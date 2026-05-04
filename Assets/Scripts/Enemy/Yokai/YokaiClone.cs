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

    public float dashSpeed;
    public AnimationCurve dashCurve;

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


    public void PerformThrust(Vector3 targetPoint)
    {
        StartCoroutine(ThrustSequence(targetPoint));
    }

    private IEnumerator ThrustSequence(Vector3 targetPoint)
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(CloneDashSequence(targetPoint));
    }

    private IEnumerator CloneDashSequence(Vector3 target)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        Vector3 dashStart = transform.position;
        float distance = Vector3.Distance(dashStart, target);
        float duration = distance / dashSpeed;
        float elapsed = 0f;

        yield return new WaitForSeconds(0.3f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = dashCurve.Evaluate(elapsed / duration);
            transform.position = Vector3.Lerp(dashStart, target, t);
            yield return null;
        }

        transform.position = target;

        // Only damage if player is grounded — jumping escapes this
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        bool playerIsGrounded = playerMovement == null || playerMovement.isGrounded;

        if (playerIsGrounded)
        {
            Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 3f, playerLayer);
            foreach (Collider hit in hitPlayers)
            {
                hit.GetComponentInParent<PlayerHealth>()?.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(0.5f);
        if (rb != null) rb.isKinematic = false;
    }

    public void CleanUp()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
