using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using System;
using UnityEngine.EventSystems;

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextDialogueIndex; // The index of the next dialogue entry to jump to
    public string action; // The name of the method to call (optional)
}

[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public string[] sentences;
    public List<DialogueChoice> choices;
    public bool end = false;
    public string action; // The name of the method to call (optional)
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
    private int lastSelectedIndex = -1; // Add this as a class member to track the last selected index
    private bool inChoiceMode = false;
    private Dictionary<string, Sprite> characterAvatars;
    public Sprite npcSprite;
    public Sprite playerSprite;
    public Sprite natSprite;
    public Sprite satoshiNakamotoSprite;
    public Sprite fedSprite;
    
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        avatar = GetComponent<Image>();
        characterAvatars = new Dictionary<string, Sprite> {
                { "NPC", npcSprite },
                { "John Galt", playerSprite },
                { "NAT", natSprite },
                { "Satoshi Nakamoto", satoshiNakamotoSprite},
                { "Agent", fedSprite}
        };
    }

    void StartDialogue()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFilePath);
        Debug.Log(path);

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

            // Display the avatar for the character
            DisplayAvatar(entry.characterName);

            foreach (string sentence in entry.sentences)
            {
                sentences.Enqueue(sentence);
            }

            // Display the first sentence
            DisplayNextSentence();

            // Execute the method associated with the entry, if specified
            if (!string.IsNullOrEmpty(entry.action))
            {
                Invoke(entry.action, 0f); // Use Invoke to call the method by name
            }
        }
        else
        {
            Debug.LogError("Dialogue entry not found at index: " + entryIndex);
        }
    }

    // This method displays the avatar corresponding to the given character name
    void DisplayAvatar(string characterName) {
        if (characterAvatars.TryGetValue(characterName, out Sprite avatarSprite)) {
            // If the character name is found in the dictionary, set the avatar image
            avatar.enabled = true;
            avatar.sprite = avatarSprite;
        } else {
            // If the character name is not found, disable the avatar image
            if (avatar != null)
                avatar.enabled = false;
            Debug.LogWarning("Avatar not found for character: " + characterName);
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

        // Select the first choice button by default
        if (choiceButtons.Count > 0)
        {
            var firstButton = choiceButtons[0].GetComponent<Button>();
            firstButton.Select();
            currentChoiceIndex = 0;
            SelectChoice(currentChoiceIndex);
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
        // Execute the method associated with the choice, if specified
        if (!string.IsNullOrEmpty(choice.action))
        {
            Invoke(choice.action, 0f); // Use Invoke to call the method by name
        }

        // Ensure that any ongoing dialogue display is stopped before proceeding
        StopAllCoroutines();

        // Update the current entry index based on the choice made
        int nextIndex = choice.nextDialogueIndex;

        // Clear any existing choices
        ClearChoices();

        // Reset the sentence index
        currentSentenceIndex = 0;

        // Check if the next dialogue index is valid before displaying
        if (nextIndex >= 0 && nextIndex < dialogueData.entries.Count)
        {
            currentEntryIndex = nextIndex; // Only update the currentEntryIndex if the nextIndex is valid
            DisplayDialogue(currentEntryIndex);
        }
        else
        {
            Debug.LogError("Invalid next dialogue index: " + nextIndex);
            inChoiceMode = false;
            EndDialogue(); // End the dialogue if the next index is invalid
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
            //inChoiceMode = false;
        }
        else
        {
            DialogueEntry currentEntry = dialogueData.entries[currentEntryIndex];

            // Check if there are choices or if the dialogue entry is marked as the end of a branch.
            if (!currentEntry.end)
            {
                // If there are choices at the end of this dialogue entry
                if (currentEntry.choices != null && currentEntry.choices.Count > 0)
                {
                    // If there are choices, display them
                    DisplayChoices(currentEntry.choices);
                    waitingForPlayerInput = true; // The player now needs to make a choice.
                }
                else
                {
                    // If there are no choices, proceed to the next dialogue entry
                    ProceedToNextDialogueEntry();
                }
            }
            else
            {
                // If the current entry is the end of a branch, do not proceed
                EndDialogue();
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
            if (waitingForPlayerInput)
            {
                if (choiceButtons.Count > 0 || inChoiceMode)
                {
                    // Only handle choice navigation if there are choices present
                    HandleChoiceNavigation();
                }
                else if (Input.GetKeyDown(KeyCode.Return) && !inChoiceMode)
                {
                    // If there are no choices, we should display the next sentence
                    DisplayNextSentence();
                }
            }
        }
    }

    private void HandleChoiceNavigation()
    {
        if (!inChoiceMode) {
            currentChoiceIndex = 0;
            inChoiceMode = true;
            SelectChoice(currentChoiceIndex);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Move selection to the right
            currentChoiceIndex++;
            currentChoiceIndex = currentChoiceIndex % choiceButtons.Count;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Move selection to the left
            currentChoiceIndex--;
            if (currentChoiceIndex < 0)
                currentChoiceIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // Execute the selected choice
            Debug.Log($"Choice selected: {currentChoiceIndex}");
            
            try {
                ChoiceButton selectedChoiceButton = choiceButtons[currentChoiceIndex].GetComponent<ChoiceButton>();
                if (selectedChoiceButton != null)
                {
                    MakeChoice(selectedChoiceButton.Choice);
                }
                else
                {
                    Debug.LogError("Selected button does not have a ChoiceButton component.");
                }
            } catch (ArgumentOutOfRangeException e) {
                inChoiceMode = false;
            }
        }

        // Update visual selection
        SelectChoice(currentChoiceIndex);
    }

    private void SelectChoice(int index)
    {
        // Ensure we have a valid index
        if (index < 0 || index >= choiceButtons.Count) return;

        // Deselect all buttons and reset text changes if any
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            var button = choiceButtons[i].GetComponent<Button>();
            button.colors = ColorBlock.defaultColorBlock; // Set to default colors
            if (i != lastSelectedIndex) // Only reset text if this button wasn't the last selected one
            {
                var buttonText = button.GetComponentInChildren<Text>();
                buttonText.text = buttonText.text.Trim('<', '>'); // Remove the angle brackets if present
            }
        }

        // If the selected index has changed, update the text
        if (lastSelectedIndex != index)
        {
            var selectedButton = choiceButtons[index].GetComponent<Button>();
            var colors = selectedButton.colors;
            colors.normalColor = Color.yellow; // Or any color that indicates selection
            colors.highlightedColor = Color.yellow;
            selectedButton.colors = colors;
            var buttonText = selectedButton.GetComponentInChildren<Text>();
            buttonText.text = "<" + buttonText.text + ">";
            selectedButton.Select();
            EventSystem.current.SetSelectedGameObject(choiceButtons[index].gameObject);

            lastSelectedIndex = index; // Update the last selected index
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

    private void OnYesChosen()
    {
        Debug.Log("Yes was chosen");
        // Add your logic here
    }

    private void OnNoChosen()
    {
        Debug.Log("No was chosen");
        // Add your logic here
    }
}
