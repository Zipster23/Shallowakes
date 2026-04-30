using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    
    // --- REFERENCES --- //

    [Header("References")]
    public TenguAI tenguAI;                     // reference to TenguAI script so we can parry the Tengu
    public SusanooAI susanooAI;                 // reference to SusanooAI script so we can parry Susanoo
    private PlayerInputHandler inputHandler;    // reference to the input handler so we can check if the player parried
    private Animator animator;                  // controls which animations play on the player
    private PlayerVFXManager vfx;               // handles parry visual effects (sparks)
    private PlayerSFXManager sfx;               // handles parry sound effects (sword clash)


    // --- PARRY SETTINGS --- //

    [Header("Parry Settings")]
    public float parryWindow = 0.2f;        // how long in seconds the parry window stays open after pressing F
    public float parryCooldown = 1f;        // how long the player has to wait before they can parry again
    public float parryRange = 4f;           // how close the enemy needs to be for a parry to be successful

    private bool isParrying = false;        // true while the parry window is open, false when it closes
    private float parryCooldownTimer = 0f;  // counts down every frame, player can parry again when it hits 0




    // --- SETUP --- //

    private void Awake()
    {
        
        // grab all required components on this GameObject
        inputHandler = GetComponent<PlayerInputHandler>();
        animator = GetComponent<Animator>();
        vfx = GetComponent<PlayerVFXManager>();
        sfx = GetComponent<PlayerSFXManager>();
        
    }




    // --- MAIN LOOP --- //

    private void Update()
    {
        
        // count down the cooldown timer every frame to track when player can parry again
        parryCooldownTimer -= Time.deltaTime;

        // if the player presses F and the cooldown is done and they're not already parrying, open the parry window
        if(inputHandler.parryInput && parryCooldownTimer <= 0f && !isParrying)
        {
            // play the parry animation
            animator.SetTrigger("Parry");

            // start the parry window coroutine
            StartCoroutine(ParryWindow());
        }

        // if the parry window is open and the Tengu exists, check for a successful parry every frame
        if(isParrying && tenguAI != null)
        {
            // calculate how far away the Tengu is
            float distance = Vector3.Distance(transform.position, tenguAI.transform.position);

            // if the Tengu is attacking and is close enough, the parry is successful
            if(tenguAI.isAttackActive && distance <= parryRange)
            {
                SuccessfulParry();
            }
        }

        // if the parry window is open and Susanoo exists, check for a successful parry every frame
        if(isParrying && susanooAI != null)
        {
            // calculate how far away the Tengu is
            float distance = Vector3.Distance(transform.position, susanooAI.transform.position);

            // if the Tengu is attacking and is close enough, the parry is successful
            if(susanooAI.isAttackActive && distance <= parryRange)
            {
                SuccessfulParry();
            }
        }

        // check for wind slash projectiles in parry range
        if(isParrying)
        {
            Collider[] nearbyProjectiles = Physics.OverlapSphere(transform.position, parryRange * 2f);
            foreach(Collider col in nearbyProjectiles)
            {
                WindSlash windSlash = col.GetComponent<WindSlash>();
                if(windSlash != null && windSlash.isParriable)
                {
                    // parry the wind slash
                    vfx.EmitParryParticles();
                    sfx.playKatanaDeflectSFX();
                    windSlash.GetParried();
                    isParrying = false;
                    parryCooldownTimer = parryCooldown;
                    return;
                }
            }
        }
    }




    // --- PARRY LOGIC --- //

    // opens the parry window for parryWindow seconds then closes it
    private IEnumerator ParryWindow()
    {
        
        isParrying = true;                              // open the parry window
        yield return new WaitForSeconds(parryWindow);   // wait for the parry window
        isParrying = false;                             // close the parry window
        parryCooldownTimer = parryCooldown;             // start the cooldown so the player can't spam parry

    }


    // called when the player successfully parries the enemy's attack
    private void SuccessfulParry()
    {
        
        isParrying = false;                 // close the parry window immediately
        parryCooldownTimer = parryCooldown; // start the cooldown so the player can't spam parry

        // play the parry VFX and SFX
        vfx.EmitParryParticles();
        sfx.playKatanaDeflectSFX();

        // knock the player back when they successfully parry
        GetComponent<PlayerMovement>().Knockback(6f, 0.2f);

        // handle Tengu parry
        if(tenguAI != null && tenguAI.isAttackActive)
        {
            if(tenguAI.isDoingCombo)
                tenguAI.comboSlashParried = true;
            else
                tenguAI.GetParried();
        }

        // handle Susanoo parry
        if(susanooAI != null && susanooAI.isAttackActive)
        {
            susanooAI.GetParried();
        }

    }

}
