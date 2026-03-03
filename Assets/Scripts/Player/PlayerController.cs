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
    public LayerMask enemyLayers;

    public float attackRange = 0.5f;
    public int attackDamage = 25;



    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerAnimatorController>();
    }

    public void Update()
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



    // Method to detect collisions
    private void Attack()
    {
        // Detect all enemies in range of the attack
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach(Collider enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }





}


