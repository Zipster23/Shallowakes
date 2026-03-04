using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float speed = 10f;
    [SerializeField] public float sprintScalar = 1.5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpForce = 350f;
    [SerializeField] private float doubleJumpForce = 500f;
    [SerializeField] private LayerMask groundLayer; // Set this in Inspector
    [SerializeField] private Transform groundCheck; // Create an empty child object at player's feet

    private int jumpsRemaining;
    private int maxJumps = 2; // Allow for double jump
    private bool isGrounded;

    public float CurrentSpeed { get; private set; }

    private Transform camTransform;

    private Rigidbody rb;

    private void Awake()
    {
        camTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Check if we are touching the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.05f, groundLayer);

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }
    }

    public void Jump()
    {
        jumpsRemaining--;

        if (jumpsRemaining > 0)
        {
            // Reset vertical velocity so the second jump always feels consistent
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (jumpsRemaining == maxJumps - 1)
            {
                rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            
        }
    }

    public void Move(Vector2 input, bool isSprinting)
    {
        if (camTransform == null) return;

        float horizontalInput = input.x;
        float forwardInput = input.y;

        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * forwardInput) + (camRight * horizontalInput);

        CurrentSpeed = moveDirection.magnitude;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition;
            
            targetPosition = moveDirection.normalized * speed * Time.deltaTime;

            if (isSprinting) 
            {
                targetPosition *= sprintScalar;
            }
            
            targetPosition += rb.position;

            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }
    }
}

