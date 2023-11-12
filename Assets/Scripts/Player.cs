using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    private bool facingRight = false;
    private Rigidbody2D rigidBody;
    [SerializeField] private float velocityTolerance = 0.1f;
    public Animator animator;
    public DialogueSystem dialogueSystem;
    public Sprite avatar;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        AnimatorController();
    }

    void Move() {
        // Get input from both horizontal and vertical axes
        float moveX = Input.GetAxis("Horizontal") * speed;
        float moveY = Input.GetAxis("Vertical") * speed;

        // Apply the movement to the Rigidbody2D
        rigidBody.velocity = new Vector2(moveX, moveY);

        // Handle character flipping for horizontal movement
        // if (moveX > 0 && !facingRight) {
        //     Flip();
        // } else if (moveX < 0 && facingRight) {
        //     Flip();
        // }
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

    private void Flip() {
        facingRight = !facingRight;
        
        // Flip the sprite by scaling along the x-axis.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Exit") {
            dialogueSystem.StartDialogueFromExternal("Scene2/exit.json", null, avatar);
            MoveToRelativePosition(new Vector2(1, 0), .5f);
        }
        if (collision.gameObject.tag == "Bathroom") {
            dialogueSystem.StartDialogueFromExternal("Scene2/bathroom.json", null, avatar);
            MoveToRelativePosition(new Vector2(0, 1), .5f);
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
