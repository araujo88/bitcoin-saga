using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    public DialogueChoice Choice;

    private DialogueSystem dialogueSystem;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>(); // Find the DialogueSystem instance in the scene
        GetComponent<Button>().onClick.AddListener(() => dialogueSystem.MakeChoice(Choice));
    }

    public void Initialize(DialogueChoice choice)
    {
        Choice = choice;
        GetComponentInChildren<Text>().text = choice.text; // Assuming each button has a child Text component to display the choice
    }

    public void OnClick()
    {
        dialogueSystem.MakeChoice(Choice);
    }
}

