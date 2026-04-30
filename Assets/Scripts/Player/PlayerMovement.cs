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
    [HideInInspector] public float attackMovementMultiplier = 1f;
    [SerializeField] private float dashCooldown = 2f;
    private float dashCooldownTimer;

    [Header("Glide Settings")]
    // terminal velocity is the constant fall speed the player reaches after ~1 second of gliding
    // drag coefficient k is derived from it: at terminal velocity, drag = gravity, so k = g / vt
    // this means you only need to set glideTerminalVelocity in the inspector ? k is calculated automatically
    [SerializeField] private float glideTerminalVelocity = 3f;      // the constant downward speed the player settles into after ~1 second of gliding
    [SerializeField] private float glideHorizontalSpeed = 8f;       // the target horizontal speed the player steers toward while gliding
    [SerializeField] private float glideTurnSpeed = 3f;             // how quickly the player can steer their glide direction (higher = snappier turns)
    [SerializeField] private float glideHorizontalDrag = 2f;        // drag coefficient applied horizontally so momentum bleeds off when changing direction

    // TODO: replace glideCooldownTimer with a reference to the global cooldown system when implemented
    // e.g. if (GlobalCooldownManager.CanUse(AbilityType.Glide)) { ... }
    [SerializeField] public float glideCooldown = 2f;   // how long the player must wait before gliding again after releasing the glide
    private float glideCooldownTimer = 0f;              // counts down every frame, player can glide again when it hits 0

    private Vector3 glideDirection = Vector3.zero;          // current horizontal steering direction, lerped over time so turning bleeds momentum
    public bool isGliding { get; private set; } = false;   // true while the player is actively gliding

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

        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Countdown the air control window
        if (airControlTimer > 0)
        {
            airControlTimer -= Time.deltaTime;
        }

        // Tick down glide cooldown
        if (glideCooldownTimer > 0)
        {
            glideCooldownTimer -= Time.deltaTime;
        }

        // Cancel glide if the player lands
        if (isGrounded && isGliding)
        {
            ExitGlide();
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
        if(isKnockedBack)
        {
            return;
        }

        // Ends the Method if the player is dashing
        if (isDashing)
        {
            return;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition = moveDirection * baseSpeed * attackMovementMultiplier * Time.deltaTime;
            if (isSprinting) targetPosition *= SpeedScale;

            rb.MovePosition(rb.position + targetPosition);

            // Only allow turning if not locked into attack
            if (attackMovementMultiplier > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
            }

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

    // --- GLIDE --- //

    // called every frame by PlayerController while the player holds G in the air
    public void Glide(Vector2 input)
    {
        // can't glide if grounded, dashing, or waiting on cooldown
        if (isGrounded || isDashing || glideCooldownTimer > 0) return;

        // on the first frame of glide, disable gravity so we can manually control descent
        // seed glideDirection from lockedAirDirection so the player doesn't snap on entry
        if (!isGliding)
        {
            isGliding = true;
            rb.useGravity = false;
            glideDirection = lockedAirDirection.magnitude > 0.1f ? lockedAirDirection : transform.forward;
        }


        // --- VERTICAL: drag-based terminal velocity --- //

        // derive drag coefficient k from terminal velocity: at vt, drag = gravity, so k = g / vt
        // applying F_drag = -k * vy each frame causes vy to converge on -vt exponentially
        // with k tuned to vt, the player reaches ~63% of terminal velocity in 1/k seconds (~1s)
        float gravity = Mathf.Abs(Physics.gravity.y);
        float k = gravity / glideTerminalVelocity;
        float dragForceVertical = -k * rb.velocity.y;   // opposes current vertical velocity
        rb.AddForce(Vector3.up * dragForceVertical, ForceMode.Acceleration);

        // also apply gravity manually so descent actually starts
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);


        // --- HORIZONTAL: steering with momentum bleed --- //

        // build the input direction from camera space the same way Move() does
        Vector3 camForward = Vector3.ProjectOnPlane(camTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(camTransform.right, Vector3.up).normalized;
        Vector3 inputDir = (camForward * input.y + camRight * input.x).normalized;

        // if there's directional input, steer glideDirection toward it gradually
        // Slerp preserves magnitude and bleeds the turn over glideTurnSpeed seconds
        if (inputDir.magnitude > 0.1f)
        {
            glideDirection = Vector3.Slerp(glideDirection, inputDir, glideTurnSpeed * Time.deltaTime);
        }

        // apply horizontal drag to bleed off momentum that no longer aligns with glideDirection
        // this is what makes sharp turns feel like the player has to fight their existing velocity
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(-horizontalVel * glideHorizontalDrag, ForceMode.Acceleration);

        // accelerate toward the target glide speed in the current steering direction
        rb.AddForce(glideDirection * glideHorizontalSpeed, ForceMode.Acceleration);

        // face the player toward the current glide direction
        if (glideDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(glideDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }
    }

    // called when the player releases G or lands on the ground
    public void ExitGlide()
    {
        if (!isGliding) return;

        isGliding = false;
        rb.useGravity = true;   // restore normal gravity

        // start the cooldown so the player can't immediately re-enter glide
        // TODO: notify global cooldown manager here instead
        glideCooldownTimer = glideCooldown;
    }

    public void DashOutput(Vector2 input)
    {
        if(dashCooldownTimer > 0) return;

        if (!isDashing)
        {
            dashCooldownTimer = dashCooldown;
            StartCoroutine(ExecuteSegmentedDash(input));
        }
    }

    public bool CanDash()
    {
        return dashCooldownTimer <= 0 && !isDashing;
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



    public Rigidbody GetRigidbody()
    {
        return rb;
    }








    // knockback function for tengu enraged mode
    public bool isKnockedBack = false;
    
    public IEnumerator ApplyKnockback(Vector3 force, float duration)
    {
        
        isKnockedBack = true;
        rb.velocity = force;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;

    }




    // knockback for parries
    // Knocks the player backwards — called when the player gets parried
    public void Knockback(float force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(float force, float duration)
    {
        float elapsed = 0f;

        // Knock back in the opposite direction the player is facing
        Vector3 knockbackDirection = -transform.forward;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Fade the force out over the duration so it feels like a stumble
            float strength = Mathf.Lerp(force, 0f, elapsed / duration);
            rb.MovePosition(rb.position + knockbackDirection * strength * Time.deltaTime);

            yield return null;
        }
    }


}