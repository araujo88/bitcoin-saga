using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float speed = 5f;
    private Rigidbody2D rigidBody;
    [SerializeField] private float velocityTolerance = 0.1f;
    public Animator animator;
    public DialogueSystem dialogueSystem;
    public Sprite avatar;
    private float fixedZPosition;

    // Public method to set the player's ability to move
    public void SetMovementEnabled(bool enabled)
    {
        // Assuming you have a method called Move() that's responsible for moving the player
        // and an Animator Controller to handle animations
        this.enabled = enabled; // Enable or disable the Player script
        animator.SetBool("isMoving", false); // Stop movement animations
        if (!enabled)
        {
            rigidBody.velocity = Vector2.zero; // Stop any current movement
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dialogueSystem.StartDialogueFromExternal("Scene2/intro.json", null, avatar, 0.025f);
        fixedZPosition = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        AnimatorController();
        ResetZPosition();        
    }

    void ResetZPosition()
    {
        // Constantly reset the Z position to the fixed value
        Vector3 position = transform.position;
        position.z = fixedZPosition;
        transform.position = position;        
    }

    void Move() {
        // Get input from both horizontal and vertical axes
        float moveX = Input.GetAxis("Horizontal") * speed;
        float moveY = Input.GetAxis("Vertical") * speed;

        // Apply the movement to the Rigidbody2D
        rigidBody.velocity = new Vector2(moveX, moveY);
    }

    void AnimatorController() {
        float absVelX = Mathf.Abs(rigidBody.velocity.x);
        float absVelY = Mathf.Abs(rigidBody.velocity.y);
        
        if (absVelX > velocityTolerance || absVelY > velocityTolerance) {
            // Player is moving
            animator.SetBool("isMoving", true);

            if (absVelX > absVelY) {
                // Horizontal movement is dominant
                animator.SetBool("isMovingRight", rigidBody.velocity.x > 0);
                animator.SetBool("isMovingLeft", rigidBody.velocity.x < 0);
                animator.SetBool("isMovingUp", false);
                animator.SetBool("isMovingDown", false);
            } else {
                // Vertical movement is dominant
                animator.SetBool("isMovingUp", rigidBody.velocity.y > 0);
                animator.SetBool("isMovingDown", rigidBody.velocity.y < 0);
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
            }
        } else {
            // Player is not moving
            animator.SetBool("isMoving", false);
            animator.SetBool("isMovingRight", false);
            animator.SetBool("isMovingLeft", false);
            animator.SetBool("isMovingUp", false);
            animator.SetBool("isMovingDown", false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Exit") {
            dialogueSystem.StartDialogueFromExternal("Scene2/exit.json", null, avatar, 0.025f);
            MoveToRelativePosition(new Vector2(1, 0), .5f);
        }
        if (collision.gameObject.tag == "Bathroom") {
            dialogueSystem.StartDialogueFromExternal("Scene2/bathroom.json", null, avatar, 0.025f);
            MoveToRelativePosition(new Vector2(0, 1), .5f);
        }
        if (collision.gameObject.tag == "Rabbit") {
            dialogueSystem.StartDialogueFromExternal("rabbit.json", null, null, 0.025f);
            Destroy(collision.gameObject);
            GameManager.Instance.rabbitsCollected++;
            Debug.Log($"Rabbits collected: {GameManager.Instance.rabbitsCollected}");
        }        
    }

    // Method to call to move the player to a relative position
    public void MoveToRelativePosition(Vector2 relativePosition, float timeToMove)
    {
        // Calculate the absolute target position based on the current position and the relative offset
        Vector2 targetPosition = (Vector2)transform.position + relativePosition;

        // Start the coroutine to move the player
        StartCoroutine(MovePlayer(targetPosition, timeToMove));
    }

    // Coroutine remains the same as your previous implementation
    private IEnumerator MovePlayer(Vector2 targetPosition, float timeToMove)
    {
        Vector2 startPosition = transform.position;
        float timePassed = 0f;

        while (timePassed < timeToMove)
        {
            float fractionOfJourney = timePassed / timeToMove;
            transform.position = Vector2.Lerp(startPosition, targetPosition, fractionOfJourney);
            timePassed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
