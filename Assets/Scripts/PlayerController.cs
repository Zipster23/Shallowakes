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

    public Transform attackPoint;


    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerAnimatorController>();
    }

    private void Update()
    {
        // move the player based on input
        movement.Move(input.MovementInput);

        if (input.jumpInput)
        {
            movement.Jump();
        }

        if (input.MovementInput != Vector2.zero)
        {
            controller.PlayMovementAnimation(true);
        } else
        {
            controller.PlayMovementAnimation(false);
        }

        if (input.parryInput)
        {
            controller.PlayParryAnimation();
        }

        if (input.attackInput)
        {
            controller.PlayAttackAnimation(); 
            Attack();
        }
        else if(input.thrustInput)
        {
            controller.PlayThrustAnimation();
            Attack();
        }

    }




    private void Attack()
    {

        // Detect all enemies in range of the attack


        // Damage them

    }



}


