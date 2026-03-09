using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguAI : MonoBehaviour
{
    
    // --- STATES --- // 

    // All possible states the Tengu can be in
    public enum TenguState
    {
        Idle,       // standing still, waiting for player to get close
        Chase,      // running towards the player
        Attack,     // in attack range, swinging at the player
        Parry,      // parrying the player's attack
        Reposition, // backing off or strafing after an attack
        Enraged     // enraged state - faster/stronger
    }

    [Header("States")]
    // The state the Tengu is currently in (starts in Idle)
    public TenguState currentState = TenguState.Idle;


    // --- REFERENCES --- //

    [Header("References")]
    public Transform player;        // reference to the player's transform
    private Animator animator;      // controls the Tengu's animations
    private Enemy enemy;            // reference to the Enemy script (for health)
    private TenguVFXManager vfx;    // handles hit visual effects
    private TenguSFXManager sfx;    // handles hit sound effects


    // --- MOVEMENT --- //

    [Header("Movement")]
    public float chaseRange = 15f;  // how far away the player can be before the Tengu starts chasing
    public float moveSpeed = 10f;    // how fast the Tengu moves


    // --- ATTACK --- //

    [Header("Attack")]
    public Transform attackPoint;           // empty GameObject positioned in front of Tengu
    public float attackRange = 3f;        // how close the player needs to be for Tengu to attack
    public int attackDamage = 100;          // how much damage each hit deals
    public LayerMask playerLayer;           // used to detect only the player in the attack overlap sphere

    public float timeBetweenAttacks = 1.5f; // how long the Tengu waits between attacks
    private float attackTimer = 0f;         // counts down to the next attack
    private bool isAttacking = false;       // prevents the Tengu from moving or switching states mid-attack


    // --- REPOSITION --- //

    [Header("Reposition")]
    public float repositionDistance = 12f;  // how far the Tengu moves when repositioning
    public float repositionTime = 1.5f;     // how long the Tengu spends repositioning before chasing again
    private float repositionTimer = 0f;     // counts down the reposition duration
    private Vector3 repositionTarget;       // The position the Tengu is moving towards when repositioning




    // --- SETUP --- //

    private void Awake()
    {
        // grab all required components on this gameobject
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        vfx = GetComponent<TenguVFXManager>();
        sfx = GetComponent<TenguSFXManager>();
    }




    // --- MAIN LOOP --- //

    private void Update()
    {
        // stop doing anything if the Tengu is dead
        if(enemy.currentHealth <= 0)
        {
            return;
        }

        // stop doing anything if the player is dead
        if(player.GetComponent<PlayerHealth>().currentHealth <= 0)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        // run whichever state we're currently in
        switch(currentState)
        {

            case TenguState.Idle:
                HandleIdle();
                break;
            case TenguState.Chase:
                HandleChase();
                break;
            case TenguState.Attack:
                HandleAttack();
                break;
            case TenguState.Parry:
                HandleParry();
                break;
            case TenguState.Reposition:
                HandleReposition();
                break;
            case TenguState.Enraged:
                HandleEnraged();
                break;

        }

    }


    

    
    


    // -------------------------
    // STATE HANDLERS
    // -------------------------




    private void HandleIdle()
    {

        // calculate how far away the player is
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // if player is within chase range, switch to the Chase state
        if(distanceToPlayer <= chaseRange)
        {
            currentState = TenguState.Chase;
        }

    }




    private void HandleChase()
    {
        
        // calculate how far away the player is
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // rotate to face the player every frame (not on Y axis so Tengu doesn't tilt up or down)
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // if Tengu is not within attack range
        if(distanceToPlayer > attackRange)
        {
            // Tengu is too far away to attack, so keep moving towards player
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // keep the same Y position so the Tengu doesn't go into the ground or air
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

            animator.SetBool("IsMoving", true);     // play run animation
        }
        else
        {
            animator.SetBool("IsMoving", false);    // Close enough to attack, so stop playing the running animation
        }

        // If Tengu is within attack range, switch to the Attack state
        if(distanceToPlayer <= attackRange)
        {
            animator.SetBool("IsMoving", false);    // make run animation stops playing
            attackTimer = 0f; // set timer to 0 so the Tengu attacks immediately instead of waiting
            currentState = TenguState.Attack;   // switch to Attack state
        }

    }




    private void HandleAttack()
    {

        // calculate how far away the player is
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // stop run animation so Tengu stands still while attacking
        animator.SetBool("IsMoving", false); 

        // if the player ran out of range and the Tengu isn't mid-swing, go back to chasing
        if(distanceToPlayer > attackRange && !isAttacking)
        {
            currentState = TenguState.Chase;
            return;
        }

        // count down the timer every frame
        attackTimer -= Time.deltaTime;

        // only trigger an attack if the timer hit 0 and the Tengu isn't already mid-attack
        if(attackTimer <= 0f && !isAttacking)
        {
            animator.SetTrigger("Attack");      // trigger the attack animation
            
            attackTimer = timeBetweenAttacks;   // reset the timer so Tengu waits before attacking again

            isAttacking = true;                 // flag that we're mid attack so it doesn't get interrupted

            // start waiting for the attack to finish, then decide whether to reposition
            StartCoroutine(RepositionAfterAttack());    
        }

    }




    private void HandleReposition()
    {
        
        repositionTimer -= Time.deltaTime;  // count down the reposition timer every frame

        // always face the player while repositioning
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // calculate how far we are from the position the Tengu is trying to reposition to
        float distanceToTarget = Vector3.Distance(transform.position, repositionTarget);

        // If the Tengu hasn't reached the target position, keep moving towards it while playing running animation
        if(distanceToTarget > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, repositionTarget, moveSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        // Else, the Tengu has reached the target position, so stop moving and playing the running animation
        else
        {
            animator.SetBool("IsMoving", false);
        }

        // when the reposition timer runs out, go back to chasing the player
        if(repositionTimer <= 0f)
        {
            animator.SetBool("IsMoving", false);
            currentState = TenguState.Chase;
        }

    }




    private void HandleParry()
    {
        
    }




    private void HandleEnraged()
    {
        
    }








    // -------------------------
    // ATTACK HIT DETECTION
    // -------------------------

    // this function is called by an Animation Event at the frame the naginata connects with the player 
    public void Attack()
    {

        // if the Tengu is repositioning, it can't attack
        if(currentState == TenguState.Reposition)
        {
            return;
        }

        // create a sphere at the attack point and detect every collider on the player layer inside of it
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        // loop through each collider that was hit when attacking
        foreach(Collider playerHit in hitPlayers)
        {
            // Find the player's PlayerHealth script to deal damage to the player
            playerHit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);

            // spawn the hit VFX slightly above the player's position so it doesn't appear at their feet
            vfx.PlayHitEffect(playerHit.transform.position + Vector3.up * 2f);

            // play the hit sound effect
            sfx.playKatanaHitSFX();

            // debug
            Debug.Log("Tengu hit Player!");
        }

    }




    // -------------------------
    // REPOSITION LOGIC
    // -------------------------

    // waits for the attack animation to finish, then randomly decides whether to reposition
    private IEnumerator RepositionAfterAttack()
    {
        
        // wait for attack animation to finish before repositioning
        yield return new WaitForSeconds(timeBetweenAttacks);

        // attack is finished, so clear the flag so a new attack can trigger
        isAttacking = false;

        // randomly pick what to do next:
        // 0 = stay and attack again
        // 1 = back up
        // 2 = strafe left/right
        int roll = Random.Range(0, 3);

        // only reposition if the roll isn't 0
        if(roll != 0)
        {
            repositionTarget = GetRepositionTarget(roll);   // calculate where to move after the random roll (back or strafe)
            repositionTimer = repositionTime;               // reset the reposition timer
            currentState = TenguState.Reposition;           // switch to the Reposition state
        }

    }


    // calculates the world position the Tengu should move to when repositioning
    private Vector3 GetRepositionTarget(int roll)
    {

        // if the roll was a 1 (back up)
        if(roll == 1)
        {
            // calculates the direction that's directly away from the player
            Vector3 dirAway = (transform.position - player.position).normalized;

            // move a certain distance away from the current position
            Vector3 target = transform.position + dirAway * repositionDistance;

            // keep the same Y position so the Tengu doesn't go into the ground or air
            target.y = transform.position.y;

            return target;
        }
        // if the roll was a 2 (strafe left/right)
        else
        {
            // calculates the direction that's directly towards the player
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // get the perpendicular direction (sideways) using cross product
            Vector3 sideways = Vector3.Cross(dirToPlayer, Vector3.up);

            // randomly decide whether to go to the left or right
            if(Random.Range(0,2) == 0)
            {
                sideways = -sideways;
            }

            // move sideways from the current position
            Vector3 target = transform.position + sideways * repositionDistance;

            // keep the same Y position so the Tengu doesn't go into the ground or air
            target.y = transform.position.y;

            return target;
        }
    }

    








    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
