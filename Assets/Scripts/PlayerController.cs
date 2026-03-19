using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputHandler input;
    private PlayerAnimatorController controller;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerAnimatorController>();
    }

    private void Update()
    {
        // move the player based on input
        movement.Move(input.MovementInput, input.sprintInput);

        if (input.jumpInput)
        {
            movement.Jump();
        }

        // ADDED CHECK: Only play movement animation if moving AND on the ground
        if (input.MovementInput != Vector2.zero && movement.isGrounded)
        {
            controller.PlayMovementAnimation(true, input.sprintInput);
        }
        else
        {
            // If the player stops moving, OR if they are in the air, stop the run animation
            controller.PlayMovementAnimation(false, false);
        }

        if (input.parryInput)
        {
            controller.PlayParryAnimation();
        }

        if (input.attackInput)
        {
            controller.PlayAttackAnimation();
        }
        else if (input.thrustInput)
        {
            controller.PlayThrustAnimation();
        }
    }
}


