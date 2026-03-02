using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayMovementAnimation(bool isMoving)
    {
        animator.SetBool("isMoving", isMoving);
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
    public void PlayerJumping()
    {
        animator.SetTrigger("Jumping");
    }
}

