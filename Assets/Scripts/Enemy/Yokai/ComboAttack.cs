using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TenguAI;

public class ComboAttack : MonoBehaviour
{
    public Transform player;
    private Animator animator;

    public float moveSpeed = 7.5f;

    [Header("Combo-Attack Ability")]
    public bool isDoingCombo = false;       // true while combo is active, blocks player attacks
    private int comboSlashCount = 0;        // tracks which slash we're on (1,2, or 3)
    public int comboChance = 30;            // percentage chance of doing combo instead of regular attack
    [HideInInspector]
    public bool comboSlashParried = false;  // tracks if current slash was parried

    [Header("Attack")]
    public Transform attackPoint;           // empty GameObject positioned in front of Tengu
    public float attackRange = 3f;          // how close the player needs to be for Tengu to attack
    public int attackDamage = 100;          // how much damage each hit deals
    public LayerMask playerLayer;           // used to detect only the player in the attack overlap sphere

    public float timeBetweenAttacks = 1.5f; // how long the Tengu waits between attacks
    [HideInInspector]
    public float attackTimer = 0f;          // counts down to the next attack
    [HideInInspector]
    public bool isAttacking = false;        // prevents the Tengu from moving or switching states mid-attack

    public bool isAttackActive = false;     // true while Tengu is mid-swing, used by parry system to detect if attack can be parried
    public float parryStunDuration = 2f;    // how long the Tengu is stunned for after getting parried

    public float tenguParryRange = 3f;      // how close the player needs to be for the Tengu to parry

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleComboAttack();
    }

    private void HandleComboAttack()
    {

        // stop moving during combo
        animator.SetBool("IsMoving", false);

        // always face the player during combo
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // keep moving towards player during combo so they can't run away
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }

    }

    public void OnComboSlash1()
    {

        comboSlashCount = 1;
        StartCoroutine(TeleportToPlayer());

        // check if player parried, if not deal damage
        if (!comboSlashParried)
        {
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider playerHit in hitPlayers)
            {
                playerHit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);
                //vfx.PlayHitEffect(playerHit.transform.position + Vector3.up * 2f);
                //sfx.PlayNaginataHitSFX();
            }
        }

        // reset parried flag for next slash
        comboSlashParried = false;

    }

    public void OnComboSlash2()
    {

        comboSlashCount = 2;
        StartCoroutine(TeleportToPlayer());

        // check if player parried, if not deal damage
        if (!comboSlashParried)
        {
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider playerHit in hitPlayers)
            {
                playerHit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);
                //vfx.PlayHitEffect(playerHit.transform.position + Vector3.up * 2f);
                //sfx.PlayNaginataHitSFX();
            }
        }

        // reset parried flag for next slash
        comboSlashParried = false;

    }

    public void OnComboSlash3()
    {

        comboSlashCount = 3;
        StartCoroutine(TeleportToPlayer());

        // check if player parried, if not deal damage
        if (!comboSlashParried)
        {
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider playerHit in hitPlayers)
            {
                playerHit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);
                //vfx.PlayHitEffect(playerHit.transform.position + Vector3.up * 2f);
                //sfx.PlayNaginataHitSFX();
            }
        }

        // reset parried flag for next slash
        comboSlashParried = false;

    }




    public void OnComboAttackEnd()
    {
        isDoingCombo = false;
        comboSlashCount = 0;
        isAttacking = false;
        comboSlashParried = false;
        player.GetComponent<PlayerParry>().parryCooldown = 1f;
        attackTimer = timeBetweenAttacks;
    }

    private IEnumerator TeleportToPlayer(float dashDuration = 0.12f)
    {

        // position Tengu directly in front of player
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
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // play dash vfx & sfx
        //vfx.PlayDodgeEffect(transform.position + Vector3.up * 1f, transform);
        //sfx.PlayDodgeSFX();

    }
}
