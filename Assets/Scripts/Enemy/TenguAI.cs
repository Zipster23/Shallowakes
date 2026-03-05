using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguAI : MonoBehaviour
{
    
    public enum TenguState
    {
        Idle,
        Chase,
        Attack,
        Parry,
        Reposition,
        Enraged
    }

    public TenguState currentState = TenguState.Idle;

    public Transform player;
    public float chaseRange = 15f;
    public float attackRange = 3f;
    public float moveSpeed = 7f;

    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

    public Transform attackPoint;
    public float attackDamageRange = 1f;
    public int attackDamage = 25;
    public LayerMask playerLayer;

    private Animator animator;
    private Enemy enemy;

    private TenguVFXManager vfx;
    private TenguSFXManager sfx;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        vfx = GetComponent<TenguVFXManager>();
        sfx = GetComponent<TenguSFXManager>();
    }

    private void Update()
    {
        
        if(enemy.currentHealth <= 0)
        {
            return;
        }

        switch(currentState)
        {

            case TenguState.Idle:
                HandleIdle();
                break;
            case TenguState.Chase:
                HandleChase();
                break;
            case TenguState.Attack:
                HandleAttack();
                break;
            case TenguState.Parry:
                HandleParry();
                break;
            case TenguState.Reposition:
                HandleReposition();
                break;
            case TenguState.Enraged:
                HandleEnraged();
                break;

        }

    }


    public void Attack()
    {
        
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackDamageRange, playerLayer);

        foreach(Collider player in hitPlayers)
        {
            vfx.PlayHitEffect(player.transform.position + Vector3.up * 2f);
            sfx.playKatanaHitSFX();
            Debug.Log("Tengu hit Player!");
        }

    }


    private void HandleIdle()
    {
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer <= chaseRange)
        {
            currentState = TenguState.Chase;
        }

    }

    private void HandleChase()
    {
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // look at player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // move towards player
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        animator.SetBool("IsMoving", true);

        // if close enough, switch to attack
        if(distanceToPlayer <= attackRange)
        {
            animator.SetBool("IsMoving", false);
            currentState = TenguState.Attack;
        }

    }

    private void HandleAttack()
    {
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // always look at player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if(distanceToPlayer > attackRange)
        {
            currentState = TenguState.Chase;
            return;
        }

        // attack on a timer
        attackTimer -= Time.deltaTime;
        if(attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            // reset timer
            attackTimer = timeBetweenAttacks;
        }

    }

    private void HandleParry()
    {
        
    }

    private void HandleReposition()
    {
        
    }

    private void HandleEnraged()
    {
        
    }
}
