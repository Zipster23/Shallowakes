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

        if (isSprinting)
        {
            animator.SetFloat("SprintScalar", movement.SpeedScale);
        }
        else
        {
            animator.SetFloat("SprintScalar", 1.0f);
        }
    }

    // Returns the Animator so other scripts can read state info
    public Animator GetAnimator()
    {
        return animator;
    }

    // Each combo hit has its own trigger: Attack_01, Attack_02, Attack_03
    // comboIndex 0 = first hit, 1 = second hit, 2 = third hit
    public void PlayComboAttack(int comboIndex)
    {
        // Build the trigger name from the index, e.g. index 0 → "Attack_01"
        string triggerName = "Attack_0" + (comboIndex + 1);
        animator.SetTrigger(triggerName);
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

    public void PlayDashAnimation()
    {
        animator.SetTrigger("Dash");
    }

    // Checks if the animator is currently in an attack state on layer 1 (body layer)
    // We use IsTag instead of IsName because IsName breaks inside sub-state machines
    public bool IsPlayingAttack()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(1);
        return info.IsTag("Attack") && info.normalizedTime < 1f;
    }

    // Returns how far through the current attack animation we are (0 = just started, 1 = finished)
    public float GetAttackNormalizedTime()
    {
        return animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
    }
}