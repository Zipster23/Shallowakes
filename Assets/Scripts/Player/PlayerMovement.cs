using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Speed values for the player's base speed and the scale by which sprinting is faster
    [SerializeField] public float baseSpeed = 10f;
    [SerializeField] public float sprintScalar = 1.5f;

    // Movement variables that affect other aspects of movement
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpForce = 350f;
    [SerializeField] private float doubleJumpForce = 500f;

    // The strength of the horizontal propulsion during a double jump
    [SerializeField] private float doubleJumpPropulsionForce = 15f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private bool debugMode = false;

    // Air control settings to define the window of influence after a double jump
    [SerializeField] private float doubleJumpControlDuration = 0.3f;
    private float airControlTimer = 0f;
    private Vector3 lockedAirDirection;

    // Friction setting to stop the "sliding on ice" feel when landing or stopping
    [SerializeField] private float groundDeceleration = 15f;

    // Value to determine how smoothly the speed transitions between moving and stopping
    [SerializeField] private float animationSmoothSpeed = 10f;
    
    [SerializeField] private float dashPower = 0.5f;
    [SerializeField] private float dashTime = 0.5f;

    // Max speed values calculated based on base speed
    private float maxBaseSpeed;
    private float maxSprintSpeed;

    // Variables to be used for jumping logic
    private int jumpsRemaining;
    private int maxJumps = 2;
    public bool isGrounded { get; private set; }

    // Variables to track speed and scale for animations
    public float CurrentSpeed { get; private set; }
    public float SpeedScale { get; private set; }

    private Transform camTransform;
    private Rigidbody rb;


    private void Awake()
    {
        camTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        maxBaseSpeed = baseSpeed;
        maxSprintSpeed = baseSpeed * sprintScalar;

        // Ensure the Rigidbody doesn't rotate itself via physics collisions
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // Check if we are touching the ground
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer);

        if (isGrounded)
        {
            airControlTimer = 0;

            // If we just landed this frame, snap horizontal velocity to zero to prevent sliding
            if (!wasGrounded)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);

                // ONLY reset jumps the moment the player touches the ground, not every frame
                jumpsRemaining = maxJumps;
            }
        }

        // Countdown the air control window
        if (airControlTimer > 0)
        {
            airControlTimer -= Time.deltaTime;
        }
    }

    public void Jump()
    {
        if (jumpsRemaining <= 0) return;

        bool isDoubleJump = !isGrounded;
        jumpsRemaining--;

        // Reset vertical velocity so the jump force feels consistent
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (isDoubleJump)
        {
            rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

            if (lockedAirDirection.magnitude > 0.1f)
            {
                // Instant propulsion boost
                rb.AddForce(lockedAirDirection * doubleJumpPropulsionForce, ForceMode.VelocityChange);
            }

            airControlTimer = doubleJumpControlDuration;
        }
        else
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void Move(Vector2 input, bool isSprinting)
    {
        if (camTransform == null) return;

        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * input.y) + (camRight * input.x);
        moveDirection = moveDirection.normalized;

        float targetSpeed = moveDirection.magnitude * baseSpeed;
        if (isSprinting && moveDirection.magnitude > 0.1f)
        {
            targetSpeed = moveDirection.magnitude * maxSprintSpeed;
        }

        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime * animationSmoothSpeed);
        SpeedScale = CurrentSpeed / baseSpeed;

        if (isGrounded)
        {
            ApplyGroundedMovement(moveDirection, isSprinting);
        }
        else
        {
            if (moveDirection.magnitude > 0.1f && airControlTimer > 0)
            {
                lockedAirDirection = moveDirection;
            }

            ApplyAirborneMovement(moveDirection, isSprinting);
        }
    }

    private void ApplyGroundedMovement(Vector3 moveDirection, bool isSprinting)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition = moveDirection * baseSpeed * Time.deltaTime;
            if (isSprinting) targetPosition *= SpeedScale;

            rb.MovePosition(rb.position + targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));

            lockedAirDirection = moveDirection;
        }
        else
        {
            // Stop horizontal sliding when there is no input on the ground
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(-horizontalVel * groundDeceleration, ForceMode.Acceleration);

            lockedAirDirection = Vector3.zero;
        }
    }

    private void ApplyAirborneMovement(Vector3 moveDirection, bool isSprinting)
    {
        Vector3 activeDirection = (airControlTimer > 0 && moveDirection.magnitude > 0.1f) ? moveDirection : lockedAirDirection;

        if (activeDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition = activeDirection * baseSpeed * Time.deltaTime;
            if (isSprinting) targetPosition *= SpeedScale;

            rb.MovePosition(rb.position + targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(activeDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }
    }


    // Added, to be reviewed
    public void DashOutput()
    {
        StartCoroutine(Dash());
    }

    public IEnumerator Dash()
    {
       float startTime = Time.time;

        // Note for Antonio to fix the dash direction when using rb.MovePosition
       while(Time.time < startTime + dashTime)
       {
        Vector3 targetPosition = Vector3.forward * dashPower * Time.deltaTime;
        rb.MovePosition(rb.position + targetPosition);
        yield return null;
       }
    }
}