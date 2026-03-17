using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerAnimatorController : MonoBehaviour
{
    PlayerMovement movement;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    public void PlayMovementAnimation(bool isMoving, bool isSprinting)
    {
        animator.SetBool("isMoving", isMoving);
        
        if (isSprinting) {
            animator.SetFloat("SprintScalar", movement.SpeedScale);
        } else {
            animator.SetFloat("SprintScalar", 1.0f);
        }
        
    }

    // Add a jump animation
    
    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayThrustAnimation()
    {
        animator.SetTrigger("Thrust");
    }

    public void PlayParryAnimation()
    {
        animator.SetTrigger("Parry");
    }
    public void PlayJumpingAnimation()
    {
        animator.SetTrigger("Jumping");
    }
}

