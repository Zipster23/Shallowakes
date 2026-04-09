using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    // References
    private PlayerInputHandler inputHandler;
    private PlayerMovement movement;
    private PlayerHealth health;
    [SerializeField] private CheatDeathAbility cheatDeathAbility;

    // Ability List to keep track of abilities
    string[] abilities = new string[] { "idle", "cheatDeath", "glide" };

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        /*
        if(health.currentHealth < 0 && cheatDeathAbility.canCheatDeath)
        {
            cheatDeathAbility.Activate();
        }
        */


        /*
        // Glide Ability Activation
        if(inputHandler.glideInput && !movement.isGrounded)
        {
            
        }

        // Add functionality to allow the player to deactivate glide mid jump
        if(movement.isGrounded)
        {
            
        }
        */
    }

}
