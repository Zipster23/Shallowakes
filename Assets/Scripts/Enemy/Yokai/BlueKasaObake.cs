using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueKasaObake : MonoBehaviour
{
    public Transform player;
    private Animator animator;

    public float attackRange = 3f;
    public float moveSpeed = 7.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleComboAttack();
    }

    private void HandleComboAttack()
    {

        // stop moving during combo
        animator.SetBool("IsMoving", false);

        // always face the player during combo
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // keep moving towards player during combo so they can't run away
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }

    }
}
