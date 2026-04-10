using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerAnimatorController : MonoBehaviour
{
    PlayerMovement movement;
    Animator playerAnimator;
    Animator koAnimator;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    public void PlayMovementAnimation(bool isMoving, bool isSprinting)
    {
        playerAnimator.SetBool("isMoving", isMoving);
        
        if (isSprinting) {
            playerAnimator.SetFloat("SprintScalar", movement.SpeedScale);
        } else {
            playerAnimator.SetFloat("SprintScalar", 1.0f);
        }
        
    }

    // Add a jump animation
    
    public void PlayAttackAnimation()
    {
        playerAnimator.SetTrigger("Attack");
    }

    public void PlayThrustAnimation()
    {
        playerAnimator.SetTrigger("Thrust");
    }

    public void PlayParryAnimation()
    {
        playerAnimator.SetTrigger("Parry");
    }
    public void PlayJumpingAnimation()
    {
        playerAnimator.SetTrigger("Jumping");
    }

    public void PlayDashAnimation()
    {
        playerAnimator.SetTrigger("Dash");
    }

    public void PlayReviveAnimation()
    {
        koAnimator.SetTrigger("CheatingDeath");
    }
}

