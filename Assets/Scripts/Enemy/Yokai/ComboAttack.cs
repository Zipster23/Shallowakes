using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboAttack : YokaiAbility
{
    [Header("Combo Settings")]
    public int comboChance = 30;
    public float moveSpeed = 7.5f;
    [HideInInspector] public bool comboSlashParried = false;
    private bool isDoingCombo = false;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 3f;
    public int attackDamage = 100;
    public LayerMask playerLayer;

    // --- YokaiAbility overrides ---

    public override bool TryTrigger(YokaiAI ai)
    {
        if (isDoingCombo) return false;
        if (Random.Range(0, 100) >= comboChance) return false;

        StartCoroutine(ComboSequence(ai));
        return true;
    }

    public override bool OnYokaiParried()
    {
        // Parry is handled per-slash via comboSlashParried, not globally
        comboSlashParried = true;
        return true;
    }

    public override void OnInterrupted()
    {
        isDoingCombo = false;
        comboSlashParried = false;
        StopAllCoroutines();
    }

    // --- Combo sequence ---

    private IEnumerator ComboSequence(YokaiAI ai)
    {
        isDoingCombo = true;
        animator.SetBool("IsMoving", false);

        animator.SetTrigger("ComboAttack"); // triggers all 3 slashes via animation events

        // Chase player between slashes
        while (isDoingCombo)
        {
            ai.FacePlayer();
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > attackRange)
            {
                Vector3 nextPos = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                if (Physics.Raycast(nextPos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 3f, ~playerLayer))
                    nextPos.y = hit.point.y;
                transform.position = nextPos;
            }
            yield return null;
        }
    }

    // --- Animation events (same as before) ---

    public void OnComboSlash1()
    {
        StartCoroutine(TeleportToPlayer());
        if (!comboSlashParried)
        {
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider hit in hits)
                hit.GetComponentInParent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
        comboSlashParried = false;
    }

    public void OnComboSlash2()
    {
        StartCoroutine(TeleportToPlayer());
        if (!comboSlashParried)
        {
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider hit in hits)
                hit.GetComponentInParent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
        comboSlashParried = false;
    }

    public void OnComboSlash3()
    {
        StartCoroutine(TeleportToPlayer());
        if (!comboSlashParried)
        {
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider hit in hits)
                hit.GetComponentInParent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
        comboSlashParried = false;
    }

    public void OnComboAttackEnd()
    {
        isDoingCombo = false;
        comboSlashParried = false;
        yokaiAI.NotifyAbilityComplete();
    }

    private IEnumerator TeleportToPlayer(float dashDuration = 0.12f)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position - (player.position - transform.position).normalized;

        // Don't lock Y — let the raycast track the slope instead
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 lerpedPos = Vector3.Lerp(startPos, targetPos, t);

            // Raycast downward to snap to terrain surface
            if (Physics.Raycast(lerpedPos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 3f, ~playerLayer))
                lerpedPos.y = hit.point.y;

            transform.position = lerpedPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final snap at destination
        Vector3 finalPos = targetPos;
        if (Physics.Raycast(finalPos + Vector3.up * 1f, Vector3.down, out RaycastHit finalHit, 3f, ~playerLayer))
            finalPos.y = finalHit.point.y;

        transform.position = finalPos;
        yokaiAI.FacePlayer();
    }
}