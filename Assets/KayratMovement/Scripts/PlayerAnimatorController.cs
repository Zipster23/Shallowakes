using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;
    PlayerMovement movement;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Update movement animation
        float speed = movement.CurrentSpeed;
        animator.SetFloat("Speed", speed);

        // Attack input
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("Attack");
        }
    }
}

