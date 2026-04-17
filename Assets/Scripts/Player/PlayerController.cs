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
    private const int COMBO_LENGTH = 3;
    private int comboIndex = 0;
    public bool isBusy = false;

    // Instead of a simple bool, we track HOW MANY times the player clicked
    // during the current animation. We only consume one click per chain step.
    private int comboClickCount = 0;

    // ── Lunge Settings ─────────────────────────────────────────────────
    [SerializeField] private float lungeForce = 6f;
    [SerializeField] private float lungeDuration = 0.15f;

    // ── Hitstop Settings ───────────────────────────────────────────────
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
        movement.Move(input.MovementInput, input.sprintInput);

        if (input.jumpInput)
            movement.Jump();

        if (input.MovementInput != Vector2.zero)
            controller.PlayMovementAnimation(true, input.sprintInput);
        else
            controller.PlayMovementAnimation(false, false);

        // ── Attack Input ───────────────────────────────────────────────
        // We use GetKeyDown directly here instead of going through
        // PlayerInputHandler so we get exactly one event per physical click.
        // This is the key fix — attackInput staying true for 0.1s was causing
        // the buffer to get set multiple times per click.
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!isBusy)
            {
                // Not attacking at all — start the combo
                StartComboAttack();
            }
            else
            {
                // Mid-attack — count this click as a buffer for the next hit
                // We use a counter instead of a bool so rapid clicks don't
                // accidentally skip a hit in the chain
                comboClickCount++;
            }
        }

        // ── Thrust ────────────────────────────────────────────────────
        if (!isBusy && input.thrustInput)
        {
            isBusy = true;
            StartCoroutine(ComboAttackCoroutine());
            controller.PlayThrustAnimation();
        }

        // ── Jump ──────────────────────────────────────────────────────
        if (input.jumpInput)
            controller.PlayJumpingAnimation();

        // ── Dash ──────────────────────────────────────────────────────
        if (input.dashInput)
        {
            controller.PlayDashAnimation();
            movement.DashOutput(input.MovementInput);
            vfx.PlayDashEffect(transform.position + Vector3.up * 0.5f, transform);
            sfx.PlayDashSFX();
        }

        // ── Glide ─────────────────────────────────────────────────────
        if (input.glideInput && !movement.isGrounded)
            movement.Glide(input.MovementInput);
        else if (movement.isGliding)
            movement.ExitGlide();
    }

    // ── Combo Logic ───────────────────────────────────────────────────

    private void StartComboAttack()
    {
        // Double safety check — should never be called while busy
        if (isBusy) return;

        comboIndex = 0;
        comboClickCount = 0;
        isBusy = true;
        controller.PlayComboAttack(comboIndex);
        StartCoroutine(ComboAttackCoroutine());
    }

    private IEnumerator ComboAttackCoroutine()
    {
        movement.attackMovementMultiplier = 0f;
        StartCoroutine(AttackLunge());

        // ── Wait for animator to enter the attack state ────────────────
        float waitTimer = 0f;
        while (!controller.IsPlayingAttack() && waitTimer < 0.2f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        if (!controller.IsPlayingAttack())
        {
            ResetCombo();
            yield break;
        }


        int clickCountAtAnimStart = comboClickCount;
        bool chained = false;

        // ── Wait for animation to finish OR for a click ────────────────
        // Every frame we check two things:
        // 1. Did the player click? → chain immediately, don't wait for anim to finish
        // 2. Did the animation finish? → end combo or chain if click already buffered
        while (controller.IsPlayingAttack() && controller.GetAttackNormalizedTime() < 0.95f)
        {
            // Player clicked during this animation — chain right away
            if (comboClickCount > clickCountAtAnimStart && comboIndex < COMBO_LENGTH - 1)
            {
                comboIndex++;
                chained = true;
                controller.PlayComboAttack(comboIndex);

                // Don't break out yet — wait for the current animation to reach
                // a good transition point (40%) so it doesn't look jarring
                while (controller.GetAttackNormalizedTime() < 0.65f)
                {
                    yield return null;
                }

                // Now start the next hit's coroutine and exit this one
                StartCoroutine(ComboAttackCoroutine());
                yield break;
            }

            yield return null;
        }

        // ── Animation finished without a mid-anim click ────────────────
        if (!chained)
        {
            StartCoroutine(RestoreMovementGradually());
        }
    }

    private IEnumerator RestoreMovementGradually()
    {
        float elapsed = 0f;
        float restoreDuration = 0.3f;

        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            movement.attackMovementMultiplier = Mathf.Lerp(0f, 1f, elapsed / restoreDuration);
            yield return null;
        }

        movement.attackMovementMultiplier = 1f;
        ResetCombo();
    }

    // Called externally by PlayerHealth when the player gets parried
    public void ResetCombo()
    {
        comboIndex = 0;
        comboClickCount = 0;
        isBusy = false;
        movement.attackMovementMultiplier = 1f;
    }

    // ── Lunge ─────────────────────────────────────────────────────────

    private IEnumerator AttackLunge()
    {
        float elapsed = 0f;
        Vector3 lungeDirection = transform.forward;

        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(lungeForce, 0f, elapsed / lungeDuration);
            movement.GetRigidbody().MovePosition(
                movement.GetRigidbody().position + lungeDirection * strength * Time.deltaTime
            );
            yield return null;
        }
    }

    // ── Hit Detection ─────────────────────────────────────────────────

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
            StartCoroutine(DoHitstop());

        }
    }

    // ── Hitstop ───────────────────────────────────────────────────────

    private IEnumerator DoHitstop()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(hitstopDuration);
        Time.timeScale = 1f;
    }

    public void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}