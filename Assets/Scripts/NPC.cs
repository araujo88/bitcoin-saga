using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.IO;

public class NPC : Entity
{
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite != null)
            initialSprite = spriteRenderer.sprite;
    }

    public override void StartDialogue() {
        if (repeatDialogue || !isDialogueFinished) {
            StartCoroutine(StartDialogueCoroutine());
            isDialogueFinished = true;
        }
    }

    public override IEnumerator StartDialogueCoroutine()
    {
        isTalking = true;
        UpdateSpriteBasedOnPlayerPosition(GameObject.Find("Player").transform.position);
        dialogueSystem.StartDialogueFromExternal(jsonFile, null, avatar, 0.025f);
        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete); 
        if (spriteRenderer.sprite != null)
            spriteRenderer.sprite = initialSprite;
        isTalking = false;
    }
}
