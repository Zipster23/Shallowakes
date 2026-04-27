using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokaiClone : MonoBehaviour
{
    // Set by ShadowClone.Initialize() after spawning
    private Transform player;
    private int attackDamage;
    private LayerMask playerLayer;

    private Animator animator;
    public Transform attackPoint;   // assign in clone prefab Inspector, same as real Yokai


    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Make the clone visually distinct — darker tint
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            // Only tint if the material supports color (avoids shader errors)
            if(r.material.HasProperty("_Color"))
            {
                r.material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }

        // Disable ShadowClone so clones don't spawn their own clones
        ShadowClone sc = GetComponent<ShadowClone>();
        if(sc != null) sc.enabled = false;

        // Disable YokaiAI so clones don't run their own AI loop
        YokaiAI ai = GetComponent<YokaiAI>();
        if(ai != null) ai.enabled = false;

        // Disable Enemy so clones can't take damage or trigger death logic
        Enemy enemy = GetComponent<Enemy>();
        if(enemy != null) enemy.enabled = false;
    }


    // Called by ShadowClone after spawning — passes data from the real Yokai
    public void Initialize(Transform playerTransform, int damage, LayerMask layer)
    {
        player      = playerTransform;
        attackDamage = damage;
        playerLayer  = layer;

        // Face the player immediately on spawn
        if(player != null)
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }


    // Called by ShadowClone when it's time for all clones to thrust simultaneously
    public void PerformThrust()
    {
        StartCoroutine(ThrustSequence());
    }


    private IEnumerator ThrustSequence()
    {
        // Play the attack animation
        if(animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for wind-up before checking hit
        yield return new WaitForSeconds(0.5f);

        // Only deal damage if player is grounded — jumping dodges this ability
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        bool playerIsGrounded = playerMovement == null || playerMovement.isGrounded;

        if(playerIsGrounded && attackPoint != null)
        {
            // Recalculate attack range from the real Yokai's attackRange value
            // Using a fixed overlap radius here — tune to match the real Yokai's attackRange
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, 3f, playerLayer);
            foreach(Collider hit in hitPlayers)
            {
                hit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);
            }
        }
    }
}
