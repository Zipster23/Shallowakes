using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputHandler input;
    private PlayerAnimatorController controller;

    // Collisions
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange = 0.5f;
    public int attackDamage = 25;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    // Collision VFX and SFX
    private PlayerVFXManager vfx;
    private PlayerSFXManager sfx;



    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerAnimatorController>();

        vfx = GetComponent<PlayerVFXManager>();
        sfx = GetComponent<PlayerSFXManager>();
    }

    public void Update()
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

        if(Time.time >= nextAttackTime)
        {
            if (input.attackInput)
            {
                controller.PlayAttackAnimation(); 
            }
            else if(input.thrustInput)
            {
                controller.PlayThrustAnimation();
            }
        }

        if (input.dashInput)
        {
            movement.DashOutput();
        }

    }



    // Method to detect collisions
    public void Attack()
    {
        // Detect all enemies in range of the attack
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach(Collider enemy in hitEnemies)
        {
            enemy.GetComponentInParent<Enemy>().TakeDamage(attackDamage);
            vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
            sfx.playKatanaHitSFX();
            Debug.Log("Hit " + enemy.name);
        }
    }





    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }







}


