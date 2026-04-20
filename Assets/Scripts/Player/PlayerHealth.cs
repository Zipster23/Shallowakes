using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    
    // --- HEALTH ---

    [Header("Health")]
    public int maxHealth = 100; // the maximum health the player can have
    public int currentHealth;   // the player's current health


    // --- REFERENCES
    
    [Header("References")]
    private Animator animator;  // controls which animations play on the player


    // --- TENGU PARRY --- //
    [Header("Tengu Parry Logic")]
    public float parryStunDuration = 0.5f;   // how long the player is frozen when their attack gets parried




    // --- SETUP ---

    private void Start()
    {
        currentHealth = maxHealth;              // set current health to max when the game starts
        animator = GetComponent<Animator>();    // grab the Animator component on this GameObject
    }




    // --- TAKING DAMAGE ---

    public void TakeDamage(int damage)
    {
        
        // if the player is already dead, don't do anything
        if(currentHealth <= 0)
        {
            return;
        }

        currentHealth -= damage;        // subtract the damage amount from the player's current health

        animator.SetTrigger("Hurt");    // play the hurt animation

        // if the player's health has hit 0, the player is dead
        if(currentHealth <= 0)
        {
            Die();  
        }

    }




    // --- DEATH --- //

    private void Die()
    {
        // play the death animation
        animator.SetBool("IsDead", true);

        // disable the player controller so the player can't attack after dying
        GetComponent<PlayerController>().enabled = false;

        // disable the player movement script so the player can't move after dying
        GetComponent<PlayerMovement>().enabled = false;

        // disable the player input script so the player can't do anything
        GetComponent<PlayerInputHandler>().enabled = false;

        GetComponent<PlayerParry>().enabled = false;

        // disable this script since we don't need health when the player dies
        this.enabled = false;
    }




    // --- TENGU PARRY LOGIC --- //
    
    // called by TenguAI when the Tengu successfully parries the player's attack
    public IEnumerator GetParried()
    {
        
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.enabled = false;
        playerController.isBusy = false;
        GetComponent<PlayerMovement>().attackMovementMultiplier = 1f;

        // Reset base layer to Idle and body layer to Empty
        // This stops the attack animation on the body layer immediately
        animator.Play("Idle", 0, 0f);
        animator.Play("Empty", 1, 0f);

        yield return new WaitForSeconds(parryStunDuration);

        playerController.isBusy = false;
        playerController.enabled = true;

    }



}
