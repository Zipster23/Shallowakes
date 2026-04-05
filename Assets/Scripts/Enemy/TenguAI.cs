using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
        Enraged,    // enraged state - faster/stronger
        IsParried   // briefly stunned after getting parried by the player
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
    public float attackRange = 3f;          // how close the player needs to be for Tengu to attack
    public int attackDamage = 100;          // how much damage each hit deals
    public LayerMask playerLayer;           // used to detect only the player in the attack overlap sphere

    public float timeBetweenAttacks = 1.5f; // how long the Tengu waits between attacks
    private float attackTimer = 0f;         // counts down to the next attack
    private bool isAttacking = false;       // prevents the Tengu from moving or switching states mid-attack

    public bool isAttackActive = false;     // true while Tengu is mid-swing, used by parry system to detect if attack can be parried
    public float parryStunDuration = 2f;    // how long the Tengu is stunned for after getting parried

    public int parryChance = 85;            // percentage chance the Tengu will parry the player's attack (0-100)
    public Transform playerTransform;       // reference to the player's transform for distance check
    public float tenguParryRange = 3f;      // how close the player needs to be for the Tengu to parry


    // --- REPOSITION --- //

    [Header("Reposition")]
    public float repositionDistance = 12f;  // how far the Tengu moves when repositioning
    public float repositionTime = 1.5f;     // how long the Tengu spends repositioning before chasing again
    private float repositionTimer = 0f;     // counts down the reposition duration
    private Vector3 repositionTarget;       // The position the Tengu is moving towards when repositioning
    public float dashSpeed = 40f;           // the speed of the tengu after Repositioning
    private bool dashStarted = false;       // prevents VFX and SFX from playing every frame during the dash


    // --- ENRAGED --- //

    [Header("Enraged")]
    public float enragedSpeedMultiplier = 1.5f;
    public float enragedAttackSpeedMultiplier = 1.5f;
    public float enragedDashSpeedMultiplier = 1.5f;
    private bool isEnraged = false;




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
            case TenguState.IsParried:
                HandleIsParried();
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
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

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
            // play VFX and SFX only once at the start of the dash
            if(!dashStarted)
            {
                dashStarted = true;
                vfx.PlayDashEffect(transform.position + Vector3.up * 2f, transform);
                sfx.PlayDashSFX();
            }

            transform.position = Vector3.MoveTowards(transform.position, repositionTarget, dashSpeed * Time.deltaTime);
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
            dashStarted = false;    // reset for next dash
            animator.SetBool("IsMoving", false);
            currentState = TenguState.Chase;
        }

    }




    private void HandleParry()
    {
        
        // stop moving while parrying
        animator.SetBool("IsMoving", false);

        // check if the parry animation has finished
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if(stateInfo.IsName("Parry") && stateInfo.normalizedTime >= 1f)
        {
            currentState = TenguState.Idle;
        }

    }




    private void HandleIsParried()
    {
        
        // stop moving while stunned
        animator.SetBool("IsMoving", false);

    }




    private void HandleEnraged()
    {
        // stop moving while enraged animation plays
        animator.SetBool("IsMoving", false);

        // wait for enraged animation to finish before going back to chasing
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsName("Enraged") && stateInfo.normalizedTime >= 1f)
        {
            currentState = TenguState.Chase;
        }

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

        // don't deal damage to the player if parried
        if(currentState == TenguState.IsParried)
        {
            return;
        }

        // don't deal damage while parrying
        if(currentState == TenguState.Parry)
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

        // attack has landed so it can no longer be parried
        isAttackActive = false;

    }




    // called by Animation Event at the start of the Tengu's attack swing
    // tells the parry system that the weapon is now active and can be parried
    public void EnableWeaponCollider()
    {

        isAttackActive = true;

    }




    // called by Animation Event at the end of the Tengu's attack swing
    // tells the parry system that the weapon is no longer active and cannot be parried
    public void DisableWeaponCollider()
    {

        isAttackActive = false;

    }




    // called by PlayerParry script when the player successfully parries the Tengu's attack
    public void GetParried()
    {
        
        isAttacking = false;                // cancel the current attack
        isAttackActive = false;             // weapon is no longer active
        StopAllCoroutines();                // cancel any running reposition coroutines
        animator.ResetTrigger("Attack");    // cancel the attack trigger
        animator.Play("Idle");              // snap back to idle animation 
        StartCoroutine(ParryStun());        // start the stun for getting parried

    }




    // called by the player's Attack() animation event - checks if the Tengu decides to parry
    // returns true if Tengu parried, false if not
    public bool CheckTenguParry()
    { 
        
        // don't parry if the Tengu is playing the enraged animation
        if(currentState == TenguState.Enraged)
        {
            return false;
        }
    
        // dont parry if already parrying
        if(currentState == TenguState.Parry)
        {
            return false;
        }
        
        // calculate distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        
        // only parry if player is within range
        if(distance > tenguParryRange)
        {
            return false;
        }
        
        // roll a random num between 0 and 100
        int parryRoll = Random.Range(0,100);
        
        // if roll is within parry chance, parry the attack
        if(parryRoll < parryChance)
        {
            currentState = TenguState.Parry;

            // play parry animation & visual feedback
            animator.SetTrigger("Parry");
            vfx.EmitSparkParticles();
            sfx.playKatanaDeflectSFX();

            // let the Tengu attack after parrying
            attackTimer = 0f;

            // start the player stun
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            StartCoroutine(playerHealth.GetParried());
            Debug.Log("Tengu parried player attack");
            return true;
        }

        return false;

    }




    // called after Tengu gets parried in GetParried(), waits for parryStunDuration seconds, then sends Tengu back to Idle
    private IEnumerator ParryStun()
    {
        
        currentState = TenguState.IsParried;                    // enter the parried state
        yield return new WaitForSeconds(parryStunDuration);     // wait for stun to finish
        currentState = TenguState.Idle;                         // go back to Idle state

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
        int roll = Random.Range(0, 5);

        // only reposition if the roll isn't 0
        if(roll == 1)
        {
            dashStarted = false;                            // reset before new dash
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




    // -------------------------
    // ENRAGED LOGIC
    // -------------------------

    public void EnterEnragedMode()
    {
        
        if(isEnraged)
        {
            return;
        }

        isEnraged = true;
        StopAllCoroutines();
        moveSpeed *= enragedSpeedMultiplier;
        timeBetweenAttacks /= enragedAttackSpeedMultiplier;
        dashSpeed *= enragedDashSpeedMultiplier;
        animator.SetTrigger("Enraged");
        sfx.PlayEnragedSFX();
        vfx.PlayEnragedEffect(transform.position + Vector3.up * 1.5f, transform, 7f);
        currentState = TenguState.Enraged;
        Debug.Log("Tengu is now enraged!");

    }
    








    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
