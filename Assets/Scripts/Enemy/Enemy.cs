using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {

        currentHealth = maxHealth;

    }

    public void TakeDamage(int damage)
    {

        currentHealth -= damage;

        // Play hurt animation

        if(currentHealth <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        // Die animation


        // Disable enemy
    }

}
