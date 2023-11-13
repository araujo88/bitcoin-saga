using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected Sprite initialSprite;
    public Sprite frontFacing;
    public Sprite rightFacing;
    public Sprite leftFacing;
    public Sprite backFacing;
    public Sprite avatar;
    protected SpriteRenderer spriteRenderer;
    public DialogueSystem dialogueSystem;
    private bool isPlayerNearby = false;
    protected bool isTalking = false; // should be handled by child class


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is nearby and the Enter key is pressed
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Return) && !isTalking)
        {
            StartDialogue();
        }
    }

    abstract public void StartDialogue();

    abstract public IEnumerator StartDialogueCoroutine();

    public void UpdateSpriteBasedOnPlayerPosition(Vector2 playerPosition)
    {
        Vector2 toPlayer = playerPosition - (Vector2)transform.position;
        toPlayer.Normalize(); // Normalize the vector to get the direction

        // Check the direction and update the sprite accordingly
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
        {
            // Player is more horizontal than vertical to the entity
            if (toPlayer.x > 0)
            {
                // Player is to the right
                spriteRenderer.sprite = leftFacing; // Assuming this sprite is for when the entity looks left
            }
            else
            {
                // Player is to the left
                spriteRenderer.sprite = rightFacing; // Assuming this sprite is for when the entity looks right
            }
        }
        else
        {
            // Player is more vertical than horizontal to the entity
            if (toPlayer.y > 0)
            {
                // Player is above
                spriteRenderer.sprite = backFacing; // Assuming this sprite is for when the entity looks up
            }
            else
            {
                // Player is below
                spriteRenderer.sprite = frontFacing; // Assuming this sprite is for when the entity looks down
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure the player has the tag "Player"
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
