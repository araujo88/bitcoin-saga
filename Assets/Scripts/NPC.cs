using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialSprite = spriteRenderer.sprite;
    }

    public override void StartDialogue() {
        StartCoroutine(StartDialogueCoroutine());
    }

    public override IEnumerator StartDialogueCoroutine()
    {
        isTalking = true;
        UpdateSpriteBasedOnPlayerPosition(GameObject.Find("Player").transform.position);
        dialogueSystem.StartDialogueFromExternal("Scene2/npc1.json", null, avatar, 0.025f);
        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete); 
        spriteRenderer.sprite = initialSprite;
        isTalking = false;
    }
}
