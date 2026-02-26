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

    private void FixedUpdate()
    {
        // move the player based on input
        movement.Move(input.MovementInput);

        if(input.parryInput)
        {
            controller.PlayParryAnimation();
        }

        if (input.attackInput)
        {
            controller.PlayAttackAnimation();
        }
        else if(input.thrustInput)
        {
            controller.PlayThrustAnimation();
        }

    }
}


