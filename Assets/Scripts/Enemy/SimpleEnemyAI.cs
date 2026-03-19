using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    
    public Transform player;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;

    private Animator animator;

    private Enemy enemy;

    private void Awake()
    {
        
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        
        if(enemy.currentHealth <= 0)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // look at player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if(distanceToPlayer <= attackRange)
        {
            // attack
        }
        else if(distanceToPlayer <= chaseRange)
        {
            // chase
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

    }

}
