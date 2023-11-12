using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SatoshiNakamoto : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public AudioClip dialogueSound;
    public Sprite avatar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndStartDialogue(1f));
    }

    IEnumerator WaitAndStartDialogue(float waitTime)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(waitTime);

        // After the wait, start the dialogue
        dialogueSystem.StartDialogueFromExternal("dialogue1.json", dialogueSound, avatar);
    }
}
