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
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
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
        targetPos.y = startPos.y;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        yokaiAI.FacePlayer();
    }
}