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

    // ── Attack Settings ────────────────────────────────────────────────
    public bool isBusy = false;

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
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!isBusy)
            {
                isBusy = true;
                controller.PlayRandomAttack();
                StartCoroutine(AttackCoroutine());
            }
        }

        // ── Thrust ────────────────────────────────────────────────────
        if (!isBusy && input.thrustInput)
        {
            isBusy = true;
            StartCoroutine(AttackCoroutine());
            controller.PlayThrustAnimation();
        }

        // ── Jump ──────────────────────────────────────────────────────
        if (input.jumpInput)
            controller.PlayJumpingAnimation();

        // ── Dash ──────────────────────────────────────────────────────
        if (input.dashInput && movement.CanDash())
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

    // ── Attack Logic ──────────────────────────────────────────────────

    private IEnumerator AttackCoroutine()
    {
        // Lock movement immediately
        movement.attackMovementMultiplier = 0f;

        // Wait for animator to enter the attack state BEFORE lunging
        float waitTimer = 0f;
        while (!controller.IsPlayingAttack() && waitTimer < 0.2f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // If animator never entered the attack state, bail out
        if (!controller.IsPlayingAttack())
        {
            ResetAttack();
            yield break;
        }

        // NOW start the lunge — animation and lunge happen at the same time
        StartCoroutine(AttackLunge());

        // Wait for the animation to finish
        while (controller.IsPlayingAttack() && controller.GetAttackNormalizedTime() < 0.95f)
        {
            yield return null;
        }

        StartCoroutine(RestoreMovementGradually());
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
        ResetAttack();
    }

    // Called externally by PlayerHealth when the player gets parried
    public void ResetAttack()
    {
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
            // check for TenguAI
            TenguAI tenguAI = enemy.GetComponentInParent<TenguAI>();

            if(tenguAI != null)
            {
                if(tenguAI.currentState == TenguAI.TenguState.Enraged) { isBusy = false; return; }
                if(tenguAI.CheckTenguResponse()) { isBusy = false; return; }
            }

            // check for SusanooAI
            SusanooAI susanooAI = enemy.GetComponentInParent<SusanooAI>();
            if(susanooAI != null)
            {
                if(susanooAI.CheckSusanooResponse()) { isBusy = false; return; }
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