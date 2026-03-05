using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Values with "SerializeField" are set in the inspector (with some having default values)
    // Speed values for the player's base speed and the scale by which sprinting is faster
    [SerializeField] public float baseSpeed = 10f;
    [SerializeField] public float sprintScalar = 1.5f;

    // Movement variables that affect other aspects of movement
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpForce = 350f;
    [SerializeField] private float doubleJumpForce = 500f;
    [SerializeField] private LayerMask groundLayer; // Set this in Inspector
    [SerializeField] private Transform groundCheck; // Create an empty child object at player's feet
    [SerializeField] private bool debugMode = false; // Enables developer view of data

    // Value to determine how smoothly the speed transitions between moving and stopping
    [SerializeField] private float animationSmoothSpeed = 10f;

    // Max speed sprint to be set upon Awake
    private float maxBaseSpeed;
    private float maxSprintSpeed;

    // Variables to be used for jumping logic
    private int jumpsRemaining;
    private int maxJumps = 2; // Allow for double jump
    private bool isGrounded;

    // Variable to track the actual smoothed speed value (e.g., 0 to 10, or up to 15)
    public float CurrentSpeed { get; private set; }

    // Variable to track the current scale of the speed proportional to the base speed (e.g., 0 to 1.0, or up to 1.5)
    public float SpeedScale { get; private set; }

    private Transform camTransform;

    private Rigidbody rb;

    private void Awake()
    {
        camTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        maxBaseSpeed = baseSpeed;
        maxSprintSpeed = baseSpeed * sprintScalar;
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

        // Get the horizontal and vertical inputs
        float horizontalInput = input.x;
        float forwardInput = input.y;

        // Determine the camera's forward and right directions
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // Flatten the camera vectors so the player doesn't move up or down based on camera pitch
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Calculate the direction the player should move based on input and camera angle
        Vector3 moveDirection = (camForward * forwardInput) + (camRight * horizontalInput);

        moveDirection = moveDirection.normalized;

        // Determine the target physical speed based on input and base speed
        float targetSpeed = moveDirection.magnitude * baseSpeed;

        // Use the max sprint speed if the player is currently holding the sprint button
        if (isSprinting && moveDirection.magnitude > 0.1f)
        {
            targetSpeed = moveDirection.magnitude * maxSprintSpeed;
        }

        // Smoothly transition the CurrentSpeed value towards the target physical speed over time
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime * animationSmoothSpeed);

        if (debugMode) Debug.Log(CurrentSpeed);

        // Calculate the scale of the current speed proportional to the base speed
        SpeedScale = CurrentSpeed / baseSpeed;

        // Only apply physical movement and rotation if there is actual input
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition;

            // Calculate the physical distance to move this frame
            targetPosition = moveDirection * baseSpeed * Time.deltaTime;

            // Apply the sprint multiplier to the physical movement if sprinting
            if (isSprinting)
            {
                targetPosition *= SpeedScale;
            }

            // Add the current position to get the final destination
            targetPosition += rb.position;

            // Move the Rigidbody to the new position
            rb.MovePosition(targetPosition);

            // Calculate the rotation to face the movement direction and smoothly rotate towards it
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }
    }
}