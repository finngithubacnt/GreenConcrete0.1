using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 7f;
    public bool isGrounded;


    [Header("References")]
   // public Animator animator;  // Reference to Animator
    private Rigidbody rb;

    bool Movement1;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Sprinting logic
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // Move relative to the player's forward direction
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        moveDirection *= speed;

        // Apply movement without affecting vertical velocity
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);

        // Play animations based on movement
        bool isMoving = moveX != 0 || moveZ != 0;
        //animator.SetBool("Run", isSprinting && isMoving); // Run animation
       // animator.SetBool("Walk", isMoving && !isSprinting); // Walk animation

    }


    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}


