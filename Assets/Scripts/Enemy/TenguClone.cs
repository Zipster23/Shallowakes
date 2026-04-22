using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguClone : MonoBehaviour
{
    
    // --- REFERENCES --- //

    [Header("References")]
    private Animator animator;          // controls which animations play on the clones
    private TenguVFXManager vfx;        // reference to the main Tengu's vfx manager so we can play vfx
    private TenguSFXManager sfx;        // reference to the main Tengu's sfx manager so we can play sfx

    // --- SETTINGS --- // 

    [Header("Settings")]
    public LayerMask playerLayer;       // the layer that the player is on. Used to detect if the clones hit the player
    public float attackRange = 3f;      // how close the clone needs to be for the thrust to connect
    public int attackDamage = 100;      // how much damage the clones deal




    // --- SETUP --- //

    private void Awake()
    {
        
        // grab reference components
        animator = GetComponent<Animator>();
        TenguAI mainTengu = FindObjectOfType<TenguAI>();
        vfx = mainTengu.GetComponent<TenguVFXManager>();
        sfx = mainTengu.GetComponent<TenguSFXManager>();

        // make the clone darker to distinguish from real Tengu
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(0.3f, 0.3f, 0.3f, 0.1f);
        }

        // disable the TenguAI script on the clone so it doesn't start chasing the player
        TenguAI ai = GetComponent<TenguAI>();
        if(ai != null) ai.enabled = false;

        // disable the Enemy script on the clone so it doesn't have its own health
        Enemy enemy = GetComponent<Enemy>();
        if(enemy != null) enemy.enabled = false;

    }



    // --- THRUST LOGIC --- //

    // Called by ShadowCloneSequence() in TenguAI when all clones should attack simultaneously
    public void PerformThrust(Transform player)
    {
        
        // immediately face the player before starting the thrust
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // start the thrust coroutine
        StartCoroutine(ThrustAtPlayer(player));

    }




    private IEnumerator ThrustAtPlayer(Transform player)
    {

        // face the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // play attack animation
        animator.SetTrigger("Attack");

        // wait a brief moment for the wind up part of the animation to play before the clone starts moving towards the player
        yield return new WaitForSeconds(0.25f);

        // move the clone rapidly towards the player, simulating a thrust motion
        float elapsed = 0f;
        float thrustDuration = 0.3f;                // how long the dash takes
        Vector3 startPos = transform.position;      // where the clone starts
        Vector3 targetPos = player.position;        // where the clone is heading
        targetPos.y = transform.position.y;         // keep same Y so the clone doesn't fly or fall

        // every frame, move the clone a little closer to the player
        while(elapsed < thrustDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / thrustDuration);
            yield return null;
        }

        // now that the clone has reached its destination, check if it hit the player
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // only deal damage if:
        // 1. the player is on the ground (jumping dodges this attack)
        // 2. the clone actually reached close enough to the player
        if(playerMovement != null && playerMovement.isGrounded && distanceToPlayer <= attackRange)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            vfx.PlayHitEffect(player.transform.position + Vector3.up * 2f);
            sfx.PlayNaginataHitSFX();
        }

        // // wait for the animation to finish, then destroy the clone
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);

    }






}
