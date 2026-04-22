using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator enemyAnimator;
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {

        currentHealth = maxHealth;

    }

    public void TakeDamage(int damage)
    {

        // Make it so that the enemy can't be hit when dead
        if(currentHealth <= 0)
        {
            return;
        }

        // make it so that Tengu cannot be damaged during enraged animation
        TenguAI tenguAI = GetComponent<TenguAI>();
        if(tenguAI != null && tenguAI.currentState == TenguAI.TenguState.Enraged)
        {
            return;
        }

        currentHealth -= damage;

        // Play hurt animation
        enemyAnimator.SetTrigger("Hurt");

        // check if tengu should enter enraged mode
        if(currentHealth <= 20)
        {
            GetComponent<TenguAI>()?.EnterEnragedMode();
        }

        if(currentHealth <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        // Die animation
        enemyAnimator.SetBool("IsDead", true);

        // Disable enemy 
        this.enabled = false;
        GetComponentInParent<TenguAI>().enabled = false;
    }

}