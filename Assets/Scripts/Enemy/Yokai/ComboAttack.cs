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

    // Called by Animation Event on each slash frame
    public void OnComboSlash1() => DealComboDamage();
    public void OnComboSlash2() => DealComboDamage();
    public void OnComboSlash3() => DealComboDamage();

    private void DealComboDamage()
    {
        if (comboSlashParried)
        {
            comboSlashParried = false;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider hit in hits)
            hit.GetComponentInParent<PlayerHealth>()?.TakeDamage(attackDamage);
    }

    public void OnComboAttackEnd()
    {
        isDoingCombo = false;
        comboSlashParried = false;
        yokaiAI.NotifyAbilityComplete();
    }
}