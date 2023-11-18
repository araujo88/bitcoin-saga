using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class Fed : Entity
{
    private Animator animator;
    private float fixedZPosition;
    private Queue<Vector2> moveQueue = new Queue<Vector2>();
    private bool isMoving = false;
    private float waitTimeBetweenMoves = 0.2f; // Duration of pause between movements
    public delegate void OnMovementComplete();
    public event Action MovementComplete;


    // Start is called before the first frame update
    void Start()
    {
        fixedZPosition = transform.position.z;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        if (spriteRenderer.sprite != null)
            initialSprite = spriteRenderer.sprite;
    }

    void Update()
    {
        ResetZPosition();        
        // Check if the player is nearby and the Enter key is pressed
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Return) && !isTalking)
        {
            StartDialogue();
        }        
    }    

    void ResetZPosition()
    {
        // Constantly reset the Z position to the fixed value
        Vector3 position = transform.position;
        position.z = fixedZPosition;
        transform.position = position;        
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
        dialogueSystem.StartDialogueFromExternal(Path.Combine(Application.streamingAssetsPath, "Dialogues/dialogue1.json"), null, avatar, 0.025f);
        yield return new WaitUntil(() => dialogueSystem.IsDialogueComplete); 
        if (spriteRenderer.sprite != null)
            spriteRenderer.sprite = initialSprite;
        isTalking = false;
    }

    public void MoveToPosition(Vector2 targetPosition)
    {
        StartCoroutine(MoveToPositionCoroutine(targetPosition));
    }

    public IEnumerator MoveToPositionCoroutine(Vector2 targetPosition)
    {
        Vector2 startPosition = rigidBody.position;
        while (Vector2.Distance(startPosition, targetPosition) > 0.01f)
        {
            Vector2 newPosition = Vector2.MoveTowards(startPosition, targetPosition, speed * Time.deltaTime);
            Vector2 direction = newPosition - startPosition;
            startPosition = newPosition;
            rigidBody.MovePosition(newPosition);

            // Set animator parameters based on direction
            animator.SetBool("isMoving", direction.magnitude > 0.01f);
            animator.SetBool("isMovingRight", direction.x > 0);
            animator.SetBool("isMovingLeft", direction.x < 0);
            animator.SetBool("isMovingUp", direction.y > 0);
            animator.SetBool("isMovingDown", direction.y < 0);

            yield return null;
        }

        // Reset animator parameters once the target position is reached
        animator.SetBool("isMoving", false);
        animator.SetBool("isMovingRight", false);
        animator.SetBool("isMovingLeft", false);
        animator.SetBool("isMovingUp", false);
        animator.SetBool("isMovingDown", false);
    }

    // Method to enqueue a new target position
    public void EnqueueMovePosition(Vector2 targetPosition)
    {
        moveQueue.Enqueue(targetPosition);
        if (!isMoving)
        {
            StartCoroutine(ProcessMoveQueue());
        }
    }

    // Coroutine to process the move queue
    private IEnumerator ProcessMoveQueue()
    {
        isMoving = true;
        while (moveQueue.Count > 0)
        {
            Vector2 nextPosition = moveQueue.Dequeue();
            yield return StartCoroutine(MoveToPositionCoroutine(nextPosition));
            yield return new WaitForSeconds(waitTimeBetweenMoves); // If you have a waiting time between moves
        }
        isMoving = false;

        MovementComplete?.Invoke(); // Invoke the completion event
    }
}
