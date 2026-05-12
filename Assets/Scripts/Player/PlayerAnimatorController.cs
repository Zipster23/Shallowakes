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

    public void PlayRandomAttack()
    {
        // Pick a random number between 1 and 3 and play that attack animation
        int randomIndex = Random.Range(1, 4); // Range is exclusive on max so this gives 1, 2, or 3
        string triggerName = "Attack_0" + randomIndex;
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

    public void PlayProjectileSlashAnimation()
    {
        animator.SetTrigger("ProjectileSlash");
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