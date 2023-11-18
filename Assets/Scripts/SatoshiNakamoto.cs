using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SatoshiNakamoto : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public AudioClip dialogueSound;
    public Sprite avatar;
    public ScreenFader screenFader;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndStartDialogue(1f));
    }

    IEnumerator WaitAndStartDialogue(float waitTime)
    {
        screenFader.FadeIn();
        // Wait for the specified amount of time
        yield return new WaitForSeconds(waitTime);

        // After the wait, start the dialogue
        dialogueSystem.StartDialogueFromExternal("dialogue1.json", dialogueSound, avatar, 0.05f);

        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete);

        screenFader.FadeOut();
        yield return new WaitForSeconds(2f);
        screenFader.FadeIn();

        yield return new WaitForSeconds(1f);

        dialogueSystem.StartDialogueFromExternal("narrator.json", null, avatar, 0.025f);

        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete);

        yield return new WaitForSeconds(1f);

        dialogueSystem.StartDialogueFromExternal("dialogue2.json", null, avatar, 0.025f);

        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete);

        // Call FedMove and wait for it to complete
        bool isMoveCompleted = false;
        FedMove(() => isMoveCompleted = true);
        yield return new WaitUntil(() => isMoveCompleted);

        dialogueSystem.StartDialogueFromExternal("dialogue3.json", null, avatar, 0.025f);

        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete);

        screenFader.FadeOut();

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(2);
    }

    void FedMove(Action onCompleted) 
    {
        GameObject entityObject = GameObject.FindWithTag("Fed");
        if (entityObject != null)
        {
            Fed entityInstance = entityObject.GetComponent<Fed>();
            if (entityInstance != null)
            {
                entityInstance.EnqueueMovePosition(new Vector3(7, -1, -2));
                entityInstance.EnqueueMovePosition(new Vector3(5, -1, -2));
                entityInstance.EnqueueMovePosition(new Vector3(5, 0.5f, -2));
                entityInstance.EnqueueMovePosition(new Vector3(-2.5f, 0.5f, -2));

                // Subscribe to the MovementComplete event
                entityInstance.MovementComplete += () => 
                {
                    // Unsubscribe immediately to prevent multiple calls
                    entityInstance.MovementComplete -= onCompleted;
                    onCompleted?.Invoke();
                };
            }
        }
    }
}
