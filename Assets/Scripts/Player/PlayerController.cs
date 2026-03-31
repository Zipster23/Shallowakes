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
    public bool isBusy = false;

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

        if (input.MovementInput != Vector2.zero)
        {
            controller.PlayMovementAnimation(true, input.sprintInput);
        } 
        else
        {
            controller.PlayMovementAnimation(false, false);
        }

        /* if (input.parryInput)
        {
            controller.PlayParryAnimation();
        }
        */




        if(!isBusy)
        {
            if (input.attackInput)
            {
                isBusy = true;
                controller.PlayAttackAnimation(); 
            }
            else if(input.thrustInput)
            {
                isBusy = true;
                controller.PlayThrustAnimation();
            }   
        } 




        if (input.jumpInput) {
            controller.PlayJumpingAnimation();
        }
        
        if (input.dashInput)
        {
            movement.DashOutput(input.MovementInput);
            vfx.PlayDashEffect(transform.position + Vector3.up * 1f, transform);
            sfx.PlayDashSFX();
        }

    }



    // Method to detect collisions
    public void Attack()
    {
        
        // doesn't do anything if the player is dead
        if(GetComponent<PlayerHealth>().currentHealth <= 0)
        {
            return;
        }

        // Detect all enemies in range of the attack
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        
        // Damage them
        foreach(Collider enemy in hitEnemies)
        {

            // check if Tengu parries this attack first
            TenguAI tenguAI = FindObjectOfType<TenguAI>();
            if(tenguAI != null && tenguAI.CheckTenguParry())
            {
                return; // Tengu parried, cancel the attack
            }

            enemy.GetComponentInParent<Enemy>().TakeDamage(attackDamage);
            vfx.PlayHitEffect(enemy.transform.position + Vector3.up * 2f);
            sfx.playKatanaHitSFX();
            Debug.Log("Hit " + enemy.name);
        }

        isBusy = false;
    }





    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }







}


