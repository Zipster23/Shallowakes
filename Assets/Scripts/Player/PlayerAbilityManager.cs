using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    // References
    private PlayerInputHandler inputHandler;
    private PlayerMovement movement;

    // Ability List to keep track of abilities
    string[] abilities = new string[] { "idle", "cheatDeath", "glide" };

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Glide Ability Activation
        if(inputHandler.glideInput && !movement.isGrounded)
        {
            
        }

        // Add functionality to allow the player to deactivate glide mid jump
        if(movement.isGrounded)
        {
            
        }
    }

}
