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

    [Header("Projectile Ability")]
    [SerializeField] private bool hasMuramasaBlade = false;
    public GameObject playerSlashPrefab;
    public float playerSlashSpeed = 25f;        // how fast the slash travels
    public float playerSlashCooldown = 15f;     // cooldown between uses
    private float playerSlashCooldownTimer = 0f;
    public KeyCode slashAbilityKey = KeyCode.Alpha1;

    [Header("Dash Ability")]
    public float playerDashSlashSpeed = 20f;    // how fast the dash moves the player forward
    public float playerDashSlashRange = 8f;     // max distance of the dash
    public float playerDashSlashCooldown = 10f; // cooldown
    private float playerDashSlashTimer = 0f;
    private bool isDashSlashing = false;
    public KeyCode dashSlashKey = KeyCode.Alpha2;






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


        // ── Slash Projectile ──────────────────────────────────────────
        if(hasMuramasaBlade)
        {
            
            playerSlashCooldownTimer -= Time.deltaTime;

            // fire horizontal slash on "1" Press
            if(Input.GetKeyDown(slashAbilityKey) && playerSlashCooldownTimer <= 0f && !isBusy)
            {
                FireHorizontalSlash();
                controller.PlayProjectileSlashAnimation();
                playerSlashCooldownTimer = playerSlashCooldown;
            }

            playerDashSlashTimer -= Time.deltaTime;
            if(Input.GetKeyDown(dashSlashKey) && playerDashSlashTimer <= 0f && !isBusy && !isDashSlashing)
            {
                StartCoroutine(PlayerDashSlash());
                playerDashSlashTimer = playerDashSlashCooldown;
            }

        }
        

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



    //── Projectile Slash Logic ──────────────────────────────────────────────────

    private void FireHorizontalSlash()
    {
        
        // spawn in front of the player at chest height
        Vector3 spawnPos = transform.position + transform.forward * 0.5f;
        spawnPos.y = transform.position.y + 1f;

        Vector3 direction = transform.forward;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject slash = Instantiate(playerSlashPrefab, spawnPos, rotation);
        // slash.transform.localScale = Vector3.one * 2f;
        slash.tag = "PlayerProjectile";

        WindSlash windSlash = slash.GetComponent<WindSlash>();
        if(windSlash != null)
        {
            windSlash.SetDirection(direction);
            windSlash.speed = playerSlashSpeed;
            windSlash.slashType = WindSlash.SlashType.Horizontal;
            windSlash.isParriable = false;  // susanoo handles this differently
            windSlash.damage = attackDamage;
        }

        // play attack sfx
        sfx.PlayProjectileSlashSFX();

    }




    //── Dash Slash Logic ──────────────────────────────────────────────────

    private IEnumerator PlayerDashSlash()
    {
        
        isDashSlashing = true;
        isBusy = true;

        // play attack animation
        controller.PlayDashSlashAnimation();

        // play dash vfx
        vfx.PlayDashEffect(transform.position + Vector3.up * 0.5f, transform);
        sfx.PlayDashSFX();

        // brief pause before dash
        yield return new WaitForSeconds(0.1f);

        // dash forward with slight aim assist
        Vector3 dashDirection = transform.forward;
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, playerDashSlashRange * 1.5f, enemyLayers);
        if (nearbyEnemies.Length > 0)
        {
            Vector3 dirToEnemy = (nearbyEnemies[0].transform.position - transform.position).normalized;
            dashDirection = Vector3.Lerp(transform.forward, dirToEnemy, 0.3f).normalized;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + dashDirection * playerDashSlashRange;
        targetPos.y = transform.position.y;

        float elapsed = 0f;
        float dashDuration = 0.3f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            yield return null;
        }

        sfx.PlayDashSlashSFX();

        // wait for attack animation event to fire Attack()
        // then isBusy and isDashSlashing get reset in Attack() when it finishes
        yield return new WaitForSeconds(0.5f);

        isDashSlashing = false;
        isBusy = false;

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
            // ADD THIS: Check for tutorial dummy FIRST
            if (enemy.CompareTag("TutorialDummy"))
            {
                TutorialDummy dummy = enemy.GetComponent<TutorialDummy>();
                if (dummy != null)
                {
                    dummy.OnHitByPlayer();
                    vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
                    sfx.playKatanaHitSFX();
                    StartCoroutine(DoHitstop());
                }
                continue; // Skip to next enemy, don't process regular damage
            }

            // check for TenguAI
            TenguAI tenguAI = enemy.GetComponentInParent<TenguAI>();

            if (tenguAI != null)
            {
                if (tenguAI.currentState == TenguAI.TenguState.Enraged) { isBusy = false; return; }
                if (tenguAI.CheckTenguResponse()) { isBusy = false; return; }
            }

            // check for SusanooAI
            SusanooAI susanooAI = enemy.GetComponentInParent<SusanooAI>();
            if (susanooAI != null)
            {
                if (susanooAI.CheckSusanooResponse()) { isBusy = false; return; }
            }

            // check for YokaiAI
            YokaiAI yokaiAI = enemy.GetComponentInParent<YokaiAI>();
            if (yokaiAI != null)
            {
                if (yokaiAI.CheckYokaiResponse()) { isBusy = false; return; }
            }

            enemy.GetComponentInParent<Enemy>().TakeDamage(attackDamage);
            vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
            sfx.playKatanaHitSFX();
            StartCoroutine(DoHitstop());
        }
    }




    // Called by Animation Event on the dash slash animation specifically
    // Uses a larger hit radius than normal Attack() since the player just dashed
    public void DashSlashAttack()
    {
        if (GetComponent<PlayerHealth>().currentHealth <= 0) return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange * 3f, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            // ADD THIS: Check for tutorial dummy FIRST
            if (enemy.CompareTag("TutorialDummy"))
            {
                TutorialDummy dummy = enemy.GetComponent<TutorialDummy>();
                if (dummy != null)
                {
                    dummy.OnHitByPlayer();
                    vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
                    sfx.playKatanaHitSFX();
                    StartCoroutine(DoHitstop());
                }
                continue;
            }

            SusanooAI susanooAI = enemy.GetComponentInParent<SusanooAI>();
            if (susanooAI != null && susanooAI.CheckSusanooResponse())
            {
                isBusy = false;
                isDashSlashing = false;
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