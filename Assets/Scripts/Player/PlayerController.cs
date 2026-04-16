using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputHandler input;
    private PlayerAnimatorController controller;
    private PlayerVFXManager vfx;
    private PlayerSFXManager sfx;

    // ── Attack Point & Layers ──────────────────────────────────────────
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackRange = 0.5f;
    public int attackDamage = 25;

    // ── Combo Settings ─────────────────────────────────────────────────
    // How many hits are in the combo chain (you have Attack_01, 02, 03 → 3 hits)
    private const int COMBO_LENGTH = 3;

    // Which hit we're currently on (0 = first hit, 1 = second, 2 = third)
    private int comboIndex = 0;

    // True while the player is mid-swing and movement should be locked/lunged
    public bool isBusy = false;

    // True if the player pressed attack during the combo window
    // When the current swing finishes, we check this to decide whether to continue the chain
    private bool comboBuffered = false;

    // How far through the animation the combo window opens
    // e.g. 0.6f means the player can press attack after 60% of the swing is done
    [SerializeField] private float comboWindowStart = 0.6f;

    // ── Lunge Settings ─────────────────────────────────────────────────
    [SerializeField] private float lungeForce = 6f;
    [SerializeField] private float lungeDuration = 0.15f;

    // ── Hitstop Settings ───────────────────────────────────────────────
    // How many seconds the game freezes when an attack connects
    // 0.04f = roughly 2 frames at 60fps — subtle but impactful
    [SerializeField] private float hitstopDuration = 0.04f;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerAnimatorController>();
        vfx = GetComponent<PlayerVFXManager>();
        sfx = GetComponent<PlayerSFXManager>();
    }

    public void Update()
    {
        // Always move and animate the player based on input
        movement.Move(input.MovementInput, input.sprintInput);

        if (input.jumpInput)
            movement.Jump();

        if (input.MovementInput != Vector2.zero)
            controller.PlayMovementAnimation(true, input.sprintInput);
        else
            controller.PlayMovementAnimation(false, false);

        // ── Attack Input ───────────────────────────────────────────────
        if (input.attackInput)
        {
            if (!isBusy)
            {
                // Not in any attack — start the combo from the beginning
                StartComboAttack();
            }
            else if (controller.GetAttackNormalizedTime() >= comboWindowStart)
            {
                // We're in an attack AND past the combo window — buffer the next hit
                // This is what gives that satisfying "chained" feeling in Genshin
                comboBuffered = true;
            }
        }

        // ── Thrust, Jump, Dash (unchanged) ────────────────────────────
        if (!isBusy && input.thrustInput)
        {
            isBusy = true;
            StartCoroutine(ComboAttackCoroutine());
            controller.PlayThrustAnimation();
        }

        if (input.jumpInput)
            controller.PlayJumpingAnimation();

        if (input.dashInput)
        {
            controller.PlayDashAnimation();
            movement.DashOutput(input.MovementInput);
            vfx.PlayDashEffect(transform.position + Vector3.up * 0.5f, transform);
            sfx.PlayDashSFX();
        }

        if (input.glideInput && !movement.isGrounded)
            movement.Glide(input.MovementInput);
        else if (movement.isGliding)
            movement.ExitGlide();
    }

    // ── Combo Logic ───────────────────────────────────────────────────

    // Kicks off the very first hit in the combo chain
    private void StartComboAttack()
    {
        comboIndex = 0;
        comboBuffered = false;
        isBusy = true;
        controller.PlayComboAttack(comboIndex);    // plays Attack_01
        StartCoroutine(ComboAttackCoroutine());
    }

    // This coroutine runs for the entire duration of ONE hit in the combo chain.
    // When it finishes, it either continues to the next hit (if buffered) or resets.
    private IEnumerator ComboAttackCoroutine()
    {
        // Lock movement and lunge forward at the start of each hit
        movement.attackMovementMultiplier = 0f;
        StartCoroutine(AttackLunge());

        // ── Wait for animator to enter the attack state ────────────────
        // Give the animator up to 0.2 seconds to process the trigger and transition
        float waitTimer = 0f;
        while (!controller.IsPlayingAttack() && waitTimer < 0.2f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // Safety check: if the animator never entered the attack state, clean up and bail
        if (!controller.IsPlayingAttack())
        {
            ResetCombo();
            yield break;
        }

        // ── Wait until we reach the combo window ──────────────────────
        // During this time movement is locked and we listen for combo input in Update()
        while (controller.IsPlayingAttack() && controller.GetAttackNormalizedTime() < comboWindowStart)
        {
            yield return null;
        }

        // ── Combo window is now open ───────────────────────────────────
        // Restore partial movement so the player isn't glued to the floor
        movement.attackMovementMultiplier = 0.3f;

        // Wait for the animation to fully finish
        while (controller.IsPlayingAttack() && controller.GetAttackNormalizedTime() < 1f)
        {
            yield return null;
        }

        // ── Animation finished — decide what happens next ──────────────
        bool canContinueCombo = comboBuffered && comboIndex < COMBO_LENGTH - 1;

        if (canContinueCombo)
        {
            // Advance to the next hit in the chain
            comboIndex++;
            comboBuffered = false;

            // Play the next attack animation (Attack_02, Attack_03, etc.)
            controller.PlayComboAttack(comboIndex);

            // Run this coroutine again for the next hit
            // We use StartCoroutine instead of recursion to avoid stack buildup
            StartCoroutine(ComboAttackCoroutine());
        }
        else
        {
            // No buffered input or end of combo chain — reset everything
            ResetCombo();
        }
    }

    // Cleans up all combo state so the player can start fresh
    private void ResetCombo()
    {
        comboIndex = 0;
        comboBuffered = false;
        isBusy = false;
        movement.attackMovementMultiplier = 1f;
    }

    // ── Lunge ─────────────────────────────────────────────────────────

    private IEnumerator AttackLunge()
    {
        float elapsed = 0f;

        // Lock in the forward direction at lunge start so it doesn't drift mid-swing
        Vector3 lungeDirection = transform.forward;

        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;

            // Fade the lunge force from full → zero so it feels like a burst, not a slide
            float strength = Mathf.Lerp(lungeForce, 0f, elapsed / lungeDuration);
            movement.GetRigidbody().MovePosition(
                movement.GetRigidbody().position + lungeDirection * strength * Time.deltaTime
            );

            yield return null;
        }
    }

    // ── Hit Detection ─────────────────────────────────────────────────

    // Called by an Animation Event on each attack animation at the frame the sword swings
    public void Attack()
    {
        if (GetComponent<PlayerHealth>().currentHealth <= 0)
            return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            TenguAI tenguAI = FindObjectOfType<TenguAI>();

            if (tenguAI != null && tenguAI.currentState == TenguAI.TenguState.Enraged)
            {
                isBusy = false;
                return;
            }

            if (tenguAI != null && tenguAI.CheckTenguResponse())
            {
                isBusy = false;
                return;
            }

            enemy.GetComponentInParent<Enemy>().TakeDamage(attackDamage);
            vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
            sfx.playKatanaHitSFX();

            // ── Hitstop ───────────────────────────────────────────────
            // Freeze time briefly so the hit feels heavy and satisfying
            StartCoroutine(DoHitstop());

            Debug.Log("Hit " + enemy.name);
        }

        // Note: we no longer set isBusy = false here
        // The ComboAttackCoroutine handles that when the animation finishes
    }

    // ── Hitstop ───────────────────────────────────────────────────────

    private IEnumerator DoHitstop()
    {
        // Freeze the entire game by setting timescale to nearly zero
        // We use a very small value instead of 0 so coroutines still run
        // (coroutines use unscaled time by default when using WaitForSecondsRealtime)
        Time.timeScale = 0.05f;

        // Wait in REAL time (not game time) so the freeze actually lasts hitstopDuration seconds
        // If we used WaitForSeconds here it would also be frozen and never finish!
        yield return new WaitForSecondsRealtime(hitstopDuration);

        // Restore normal time
        Time.timeScale = 1f;
    }

    public void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}