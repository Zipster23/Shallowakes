using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private TenguAI tenguAI;    // to knock tengu back when he parries player attack
    [SerializeField] private PlayerVFXManager vfx;
    [SerializeField] private PlayerSFXManager sfx;
    [SerializeField] private TenguRespawnManager tenguRespawnManager;
    [SerializeField] private SusanooRespawnManager susanooRespawnManager;
    [SerializeField] private WhisperingForestRespawnManager whisperingForestRespawnManager;


    // --- TENGU PARRY --- //
    [Header("Tengu Parry Logic")]
    public float parryStunDuration = 1.1f;
    private bool isStunned = false; // true while player is stunned from getting parried
    private float stunTimer = 0; // counts down every frame, player is re-enabled when it hits 0




    // --- SETUP ---

    private void Start()
    {
        currentHealth = maxHealth;              // set current health to max when the game starts
        animator = GetComponent<Animator>();    // grab the Animator component on this GameObject
    }




    private void Update()
    {
        
        if(isStunned)
        {
            stunTimer -= Time.deltaTime;

            if(stunTimer <= 0f)
            {
                isStunned = false;
                PlayerController playerController = GetComponent<PlayerController>();
                playerController.isBusy = false;
                playerController.enabled = true;
            }
        }

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

        vfx.ForceStopSwingEffects();
        vfx.PlayGetHitVFX(transform.position);
        sfx.PlayGetHitSFX();

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

        vfx.ForceStopSwingEffects();

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

        // trigger death and respawn sequence
        if(tenguRespawnManager != null)
        {
            tenguRespawnManager.OnPlayerDied();
        }

        if(susanooRespawnManager != null)
        {
            susanooRespawnManager.OnPlayerDied();
        }

        if (whisperingForestRespawnManager != null)
            whisperingForestRespawnManager.OnPlayerDied();
    }




    // --- TENGU PARRY LOGIC --- //
    
    // called by TenguAI when the Tengu successfully parries the player's attack
    public void GetParried()
    {
        vfx.ForceStopSwingEffects();
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.enabled = false;
        playerController.isBusy = false;
        GetComponent<PlayerMovement>().attackMovementMultiplier = 1f;

        Knockback(10f, 0.2f);

        // Reset base layer to Idle and body layer to Empty
        // This stops the attack animation on the body layer immediately
        animator.Play("Idle", 0, 0f);
        animator.Play("Empty", 1, 0f);

        isStunned = true;
        stunTimer = parryStunDuration;

    }

    public void Knockback(float force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(float force, float duration)
    {
        float elapsed = 0f;
        
        // Move backward from where the player is currently facing
        Vector3 knockbackDirection = -transform.forward;
        Rigidbody rb = GetComponent<Rigidbody>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(force, 0f, elapsed / duration);
            rb.MovePosition(rb.position + knockbackDirection * strength * Time.deltaTime);

            yield return null;
        }
    }



}
