using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using System;

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextDialogueIndex; // The index of the next dialogue entry to jump to
}

[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public string[] sentences;
    public List<DialogueChoice> choices;
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
    // private bool skipTyping = false;
    private AudioClip dialogueSound;
    private AudioSource audioSource;
    private Image avatar;
    public event Action OnDialogueEnd;
    public bool IsDialogueComplete { get; private set; }
    private float typeSeconds = 0.05f;
    public GameObject choiceButtonPrefab; // Assign this in the inspector
    private List<GameObject> choiceButtons = new List<GameObject>(); // Track the choice buttons
    private int currentChoiceIndex = 0; // Index of the selected choice button
    private bool inDialogueMode = false;
    public Player playerController; // Assign this in the inspector

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
                DisplayDialogue(currentEntryIndex);
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

    // Modify this method to accept an int index instead of a string characterName
    void DisplayDialogue(int entryIndex)
    {
        if (entryIndex < 0 || entryIndex >= dialogueData.entries.Count)
        {
            Debug.LogError("Dialogue entry index out of range: " + entryIndex);
            return;
        }

        DialogueEntry entry = dialogueData.entries[entryIndex];

        if (entry != null)
        {
            nameText.text = entry.characterName + ":";
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
            Debug.LogError("Dialogue entry not found at index: " + entryIndex);
        }
    }

    private void DisplayChoices(List<DialogueChoice> choices)
    {
        // Clear any existing choice buttons first
        ClearChoices();

        // Determine the starting position for the first button
        Vector3 startPosition = new Vector3(-172.5f, 0, 0); // You can adjust this as needed
        float xOffset = 172.5f; // The x offset between buttons, adjust as needed
        float yOffset = 30f; // The y offset between buttons, adjust as needed

        // Instantiate choice buttons based on the JSON data
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, dialoguePanelInstance.transform, false);
            buttonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + (xOffset * i), startPosition.y - yOffset);

            // Get the ChoiceButton script attached to the instantiated button
            ChoiceButton choiceBtnComponent = buttonObj.GetComponent<ChoiceButton>();

            // Initialize the button with the choice data
            if (choiceBtnComponent != null)
            {
                choiceBtnComponent.Initialize(choices[i]);
            }
            else
            {
                Debug.LogError("ChoiceButton component not found on the instantiated button prefab.");
            }

            // Add the instantiated button to the list for reference
            choiceButtons.Add(buttonObj);
        }

        // Set the first choice as selected by default, if any choices are available
        if (choiceButtons.Count > 0)
        {
            currentChoiceIndex = 0;
            SelectChoice(currentChoiceIndex); // This function needs to be implemented to handle the visual selection of buttons
        }
    }

    private void ClearChoices()
    {
        foreach (var button in choiceButtons)
        {
            Destroy(button); // Remove the button from the scene
        }
        choiceButtons.Clear();
        currentChoiceIndex = 0; // Reset the choice index
    }

    public void MakeChoice(DialogueChoice choice)
    {
        // Ensure that any ongoing dialogue display is stopped before proceeding
        StopAllCoroutines();
        
        // Update the current entry index based on the choice made
        currentEntryIndex = choice.nextDialogueIndex;

        // Clear any existing choices
        ClearChoices();

        // Reset the sentence index
        currentSentenceIndex = 0;

        // Display the dialogue entry linked to the chosen dialogue choice
        DisplayDialogue(currentEntryIndex);
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
            // Check if there are choices at the end of this dialogue entry
            if (dialogueData.entries[currentEntryIndex].choices != null &&
                dialogueData.entries[currentEntryIndex].choices.Count > 0)
            {
                // If there are choices, display them
                DisplayChoices(dialogueData.entries[currentEntryIndex].choices);
            }
            else
            {
                // If there are no choices, proceed to the next dialogue entry
                ProceedToNextDialogueEntry();
            }
        }
    }

    private void ProceedToNextDialogueEntry()
    {
        currentEntryIndex++;
        if (currentEntryIndex < dialogueData.entries.Count)
        {
            DisplayDialogue(currentEntryIndex);
        }
        else
        {
            // End of conversation
            EndDialogue();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            // Check if dialogueText is not null before trying to set its text property
            if (dialogueText != null)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typeSeconds);
            }
            else
            {
                // If dialogueText is null, exit the coroutine
                yield break;
            }
        }

        waitingForPlayerInput = true;
    }

    void EndDialogue()
    {
        // Clear choices when ending the dialogue
        ClearChoices();        
        nameText.text = "";
        dialogueText.text = "";

        // Set waitingForPlayerInput to false to indicate the dialogue has ended
        waitingForPlayerInput = false;
        dialoguePanelInstance.SetActive(false);
        dialogStarted = false;
        IsDialogueComplete = true;
        ExitDialogueMode();        
        OnDialogueEnd?.Invoke();
        StopAllCoroutines();
        Destroy(dialoguePanelInstance);
    }

    // Update is called once per frame
    void Update()
    {
        if (inDialogueMode)
        {
            if (waitingForPlayerInput && sentences.Count == 0) // Only accept choice inputs if there are no sentences left to display
            {
                if (choiceButtons.Count > 0)
                {
                    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        currentChoiceIndex = (currentChoiceIndex + 1) % choiceButtons.Count;
                        SelectChoice(currentChoiceIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        currentChoiceIndex--;
                        if (currentChoiceIndex < 0)
                        {
                            currentChoiceIndex = choiceButtons.Count - 1;
                        }
                        SelectChoice(currentChoiceIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        ExecuteChoice(currentChoiceIndex);
                    }
                }
            }
            else if (waitingForPlayerInput)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    DisplayNextSentence();
                }
            }
        }
    }

    private void SelectChoice(int index)
    {
        // Deselect all buttons
        foreach (var button in choiceButtons)
        {
            var btn = button.GetComponent<Button>();
            btn.colors = ColorBlock.defaultColorBlock; // Set to default colors
        }

        // Highlight the selected button
        var selectedButton = choiceButtons[index].GetComponent<Button>();
        var colors = selectedButton.colors;
        colors.normalColor = Color.yellow; // Or any color that indicates selection
        colors.highlightedColor = Color.yellow;
        selectedButton.colors = colors;

        // Optionally, scroll to the selected button if it's not fully visible
        // (for example, in a scroll view)
        // (Your scroll view component).ScrollTo(selectedButton);
    }

    private void ExecuteChoice(int index)
    {
        DialogueChoice choice = choiceButtons[index].GetComponent<ChoiceButton>().Choice;
        ClearChoices();
        DisplayDialogue(choice.nextDialogueIndex);
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
    public void StartDialogueFromExternal(string filename, AudioClip sound, Sprite sprite, float velocity)
    {
        if (!dialogStarted) {
            typeSeconds = velocity;
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
            EnterDialogueMode();
            StartDialogue();
        }
    }

    public void EnterDialogueMode()
    {
        inDialogueMode = true;
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
    }

    public void ExitDialogueMode()
    {
        inDialogueMode = false;
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
    }
}
