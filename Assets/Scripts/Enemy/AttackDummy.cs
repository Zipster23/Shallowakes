using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDummy : MonoBehaviour
{
    
    private Animator animator;
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        if(attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            attackTimer = timeBetweenAttacks;
        }
    }

}
