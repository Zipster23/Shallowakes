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

        currentHealth -= damage;

        // Play hurt animation
        enemyAnimator.SetTrigger("Hurt");

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
        GetComponent<TenguAI>().enabled = false;
        GetComponent<Enemy>().enabled = false;
    }

}
