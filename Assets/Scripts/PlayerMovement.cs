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

    [Header("Segmented Dash Settings")]
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float segmentSpeed = 40f;
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private int maxSegments = 4; // Point 1 to 2, 2 to 3, 3 to 4, etc.
    [SerializeField] public float temporaryScalar = 1f;
    private bool isDashing = false;

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
            ApplyGroundedMovement(moveDirection, isSprinting, isDashing);
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

    private void ApplyGroundedMovement(Vector3 moveDirection, bool isSprinting, bool isDashing)
    {
        // Ends the Method if the player is dashing
        if (isDashing)
        {
            return;
        }

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

    public void DashOutput(Vector2 input)
    {
        if (!isDashing) StartCoroutine(ExecuteSegmentedDash(input));
    }

    private IEnumerator ExecuteSegmentedDash(Vector2 input)
    {
        isDashing = true;

        // Prepare physics for manual movement
        Vector3 originalVelocity = rb.velocity;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        // Determine initial direction based on camera and input
        Vector3 currentDir = (Vector3.ProjectOnPlane(camTransform.forward, Vector3.up) * input.y +
                             Vector3.ProjectOnPlane(camTransform.right, Vector3.up) * input.x).normalized;

        if (currentDir.sqrMagnitude < 0.01f) currentDir = transform.forward;

        float remainingDist = dashDistance;
        int currentSegment = 0;

        // THE LADDER LOOP: Process one segment at a time
        while (remainingDist > 0.05f && currentSegment < maxSegments)
        {
            currentSegment++;
            Vector3 startPos = rb.position;
            Vector3 targetPos;
            bool hitWall = false;

            // Raycast from current position to find the next segment in the ladder
            // Offset slightly up (0.5f) to ensure we don't clip into flat floors
            if (Physics.Raycast(startPos + Vector3.up * 0.1f, currentDir, out RaycastHit hit, remainingDist, groundLayer))
            {

                float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);

                if (slopeAngle <= maxSlopeAngle)
                {
                    // Valid Slope: Point 2 is the hit point
                    targetPos = hit.point + Vector3.up * temporaryScalar;

                    // Move to this segment's end point
                    yield return StartCoroutine(MoveToPoint(startPos, targetPos));

                    // Update math for next segment
                    float distTraveled = Vector3.Distance(startPos, targetPos);
                    remainingDist -= distTraveled;

                    // Calculate new direction for the next segment (along the slope)
                    currentDir = Vector3.ProjectOnPlane(currentDir, hit.normal).normalized;
                }
                else
                {
                    // Steep Wall: Stop here
                    targetPos = hit.point - (currentDir * 0.2f); // Slight buffer
                    yield return StartCoroutine(MoveToPoint(startPos, targetPos));
                    remainingDist = 0;
                    hitWall = true;
                }
            }
            else
            {
                // No obstacle: Move the full remaining distance
                targetPos = startPos + (currentDir * remainingDist);
                yield return StartCoroutine(MoveToPoint(startPos, targetPos));
                remainingDist = 0;
            }

            if (hitWall) break;
        }

        // Restore physics
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        isDashing = false;
    }

    // This handles the actual movement for a single segment
    private IEnumerator MoveToPoint(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        if (distance <= 0f) yield break;

        float elapsed = 0f;
        float duration = distance / segmentSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move physics body
            rb.MovePosition(Vector3.Lerp(from, to, t));
            yield return null;
        }

        // Ensure we land exactly at the segment node
        rb.MovePosition(to);
    }
}