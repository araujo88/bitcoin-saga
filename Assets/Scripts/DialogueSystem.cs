using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using System;

[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public string[] sentences;
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueEntry> entries;
}

public class DialogueSystem : MonoBehaviour
{
    private Text nameText;
    private Text dialogueText;    
    private Queue<string> sentences = new Queue<string>();
    private DialogueData dialogueData;
    private int currentSentenceIndex = 0; // Track the current sentence index
    private int currentEntryIndex = 0; // Track the current entry index
    private bool waitingForPlayerInput = false;
    private string jsonFilePath;
    public GameObject dialoguePanelPrefab;
    protected GameObject dialoguePanelInstance;
    private bool dialogStarted = false;
    private bool skipTyping = false;
    private AudioClip dialogueSound;
    private AudioSource audioSource;
    private Image avatar;
    public event Action OnDialogueEnd;
    public bool IsDialogueComplete { get; private set; }


    void Start() {
        audioSource = GetComponent<AudioSource>();
        avatar = GetComponent<Image>();
    }

    void StartDialogue()
    {
        string path = Path.Combine(Application.dataPath, jsonFilePath);

        if (LoadDialogueData(path))
        {
            if (currentEntryIndex < dialogueData.entries.Count)
            {
                DisplayDialogue(dialogueData.entries[currentEntryIndex].characterName);
            }
            else
            {
                Debug.Log("No more dialogue entries to display.");
                EndDialogue();
            }
        }
        else
        {
            Debug.LogError("Failed to load dialogue data.");
        }
    }


    bool LoadDialogueData(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON file not found at path: " + jsonFilePath);
            return false;
        }

        string json = File.ReadAllText(jsonFilePath);
        dialogueData = JsonUtility.FromJson<DialogueData>(json);

        if (dialogueData != null && dialogueData.entries != null && dialogueData.entries.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void DisplayDialogue(string characterName)
    {
        DialogueEntry entry = dialogueData.entries.Find(e => e.characterName == characterName);

        if (entry != null)
        {
            nameText.text = characterName + ":";
            sentences.Clear();
            currentSentenceIndex = 0; // Reset the sentence index
            foreach (string sentence in entry.sentences)
            {
                sentences.Enqueue(sentence);
            }

            // Display the first sentence
            DisplayNextSentence();
        }
        else
        {
            Debug.LogError("Character not found in dialogue data: " + characterName);
        }
    }

    void DisplayNextSentence()
    {
        if (currentSentenceIndex < sentences.Count)
        {
            StopAllCoroutines(); // Stop any existing typing coroutine in case the player skips to the next sentence
            string sentence = sentences.ElementAt(currentSentenceIndex);
            PlaySound(dialogueSound);            
            StartCoroutine(TypeSentence(sentence)); // Start the coroutine to type out the sentence
            currentSentenceIndex++;
        }
        else
        {
            // No more sentences, check for the next entry
            currentEntryIndex++;

            if (currentEntryIndex < dialogueData.entries.Count)
            {
                DisplayDialogue(dialogueData.entries[currentEntryIndex].characterName);
            }
            else
            {
                // End of conversation
                EndDialogue();
            }
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Waits 0.05 seconds before continuing the loop, adjust the timing to your liking
        }
        waitingForPlayerInput = true; // The player can proceed to the next sentence after the current one has finished typing
    }

    void EndDialogue()
    {
        nameText.text = "";
        dialogueText.text = "";

        // Set waitingForPlayerInput to false to indicate the dialogue has ended
        waitingForPlayerInput = false;
        dialoguePanelInstance.SetActive(false);
        dialogStarted = false;
        IsDialogueComplete = true;
        OnDialogueEnd?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingForPlayerInput)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // Player pressed Enter, proceed to the next sentence
                DisplayNextSentence();
            }
        }
    }

    void PlaySound(AudioClip sound)
    {
        if (audioSource && sound)
        {
            audioSource.clip = sound;
            audioSource.Play();
        }
    }

    // You can use this method to start a dialogue from an external script
    public void StartDialogueFromExternal(string filename, AudioClip sound, Sprite sprite)
    {
        if (!dialogStarted) {
            dialogueSound = sound;
            IsDialogueComplete = false;

            dialoguePanelInstance = Instantiate(dialoguePanelPrefab);
            dialoguePanelInstance.SetActive(true);
            dialoguePanelInstance.transform.SetParent(GameObject.Find("Dialogue").transform, false);

            if (sprite != null) {
                avatar = dialoguePanelInstance.transform.Find("Image").GetComponent<Image>();
                avatar.sprite = sprite;
                avatar.enabled = true;
                avatar.color = Color.white;
            }

            nameText = dialoguePanelInstance.transform.Find("Name").GetComponent<Text>();
            dialogueText = dialoguePanelInstance.transform.Find("Dialogue").GetComponent<Text>();
            
            jsonFilePath = "Dialogues/" + filename;
            currentEntryIndex = 0;
            currentSentenceIndex = 0;
            dialogStarted = true;
            StartDialogue();
        }
    }
}
