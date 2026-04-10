using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
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
        Dodge,      // backing off or strafing after an attack
        Enraged,    // enraged state - faster/stronger
        IsParried,  // briefly stunned after getting parried by the player
        DashSlash
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

    public float tenguParryRange = 3f;      // how close the player needs to be for the Tengu to parry


    // --- RESPONSE CHANCES --- //

    [Header("Response Chances")]
    public int normalParryChance = 60;      // parry chance in normal mode
    public int normalDodgeChance = 25;      // dodge chance in normal mode
    public int enragedParryChance = 20;     // parry chance in enraged mode
    public int enragedDodgeChance = 65;     // dodge chance in enraged mode    
    public float dodgeDistance = 12f;  // how far the Tengu moves when repositioning
    private Vector3 dodgeTarget;       // The position the Tengu is moving towards when repositioning
    public float dodgeSpeed = 40f;           // the speed of the tengu after Repositioning
    private bool dodgeStarted = false;       // prevents VFX and SFX from playing every frame during the dash


    // --- ENRAGED --- //

    [Header("Enraged")]
    public float enragedSpeedMultiplier = 1.5f;         // how much faster the Tengu moves while enraged
    public float enragedAttackSpeedMultiplier = 1.5f;   // how much faster the Tengu attacks while enraged
    public float enragedDodgeSpeedMultiplier = 1.5f;     // how much faster the Tengu dashes while enraged
    private bool isEnraged = false;                     // bool to prevent Tengu from enraging multiple times
    public CinemachineImpulseSource impulseSource;      // reference to Cinemachine Impulse Source on MainCamera to generate screen shake


    // --- ABILITIES --- //

    [Header("Dash-Slash Ability")]
    public float dashSlashRange = 20f;      // distance at which Tengu triggers dash slash
    public float dashSlashSpeed = 80f;      // how fast the Tengu moves during dash slash
    private bool isDashSlashing = false;




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
            case TenguState.Dodge:
                HandleDodge();
                break;
            case TenguState.Enraged:
                HandleEnraged();
                break;
            case TenguState.IsParried:
                HandleIsParried();
                break;
            case TenguState.DashSlash:
                HandleDashSlash();
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

        // if player is too far away, do a dash slash instead of a regular chase
        if(distanceToPlayer > dashSlashRange)
        {
            currentState = TenguState.DashSlash;
            return;
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

            // double check if player is still in range before attacking
            if(distanceToPlayer > attackRange)
            {
                currentState = TenguState.Chase;
                return; 
            }
            
            animator.SetTrigger("Attack");      // trigger the attack animation
            
            attackTimer = timeBetweenAttacks;   // reset the timer so Tengu waits before attacking again

            isAttacking = true;                 // flag that we're mid attack so it doesn't get interrupted

        }

    }




    private void HandleDodge()
    {

        // always face the player while repositioning
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // calculate how far we are from the position the Tengu is trying to reposition to
        float distanceToTarget = Vector3.Distance(transform.position, dodgeTarget);

        // If the Tengu hasn't reached the target position, keep moving towards it while playing running animation
        if(distanceToTarget > 0.5f)
        {
            // play VFX and SFX only once at the start of the dash
            if(!dodgeStarted)
            {
                dodgeStarted = true;
                vfx.PlayDodgeEffect(transform.position, transform);
                sfx.PlayDodgeSFX();
            }

            transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, dodgeSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        // Else, the Tengu has reached the target position, so stop moving and playing the running animation
        else
        {
            // reached dodge target, decide what to do next
            dodgeStarted = false;
            animator.SetBool("IsMoving", false);

            int roll = Random.Range(0, 2);
            if(roll == 0)
            {
                currentState = TenguState.DashSlash;
            }
            else
            {
                currentState = TenguState.Chase;
            }
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

        // gets what's currently playing on Animator Base Layer
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); 

        // wait for enraged animation to finish before going back to chasing
        // if the Enraged animation is playing & if the Enraged animation has fully played, go back to chasing
        if(stateInfo.IsName("Enraged") && stateInfo.normalizedTime >= 1f)
        {
            currentState = TenguState.Chase;
        }

    }




    private void HandleDashSlash()
    {
        
        // Only start the sequence if we aren't already in the middle of it
        if (!isDashSlashing)
        {
            StartCoroutine(DashSlashSequence());
        }

    }

    








    // -------------------------
    // ATTACK HIT DETECTION
    // -------------------------

    // this function is called by an Animation Event at the frame the naginata connects with the player 
    public void Attack()
    {
    
        // if the Tengu is repositioning, it can't attack
        if(currentState == TenguState.Dodge)
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



    // called by Animation Event at the end of the Tengu's attack animation
    // sets isAttacking to false so the Tengu is able to attack again
    public void OnAttackEnd()
    {

        isAttacking = false;

    }



    public void OnDashSlashEffect()
    {
        // Spawns the slash at the player's height but at the Tengu's forward position
        Vector3 spawnPos = transform.position + (transform.forward * 2f) + (Vector3.up * 2f);
        vfx.PlaySlashEffect(spawnPos, transform);
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




    // called by the player's Attack() animation event - checks if the Tengu decides to parry or dodge player attack
    // returns true if Tengu parried/dodged, false if not
    public bool CheckTenguResponse()
    { 

        int currentParryChance;
        if(isEnraged)
        {
            currentParryChance = enragedParryChance;
        }
        else
        {
            currentParryChance = normalParryChance;
        }

        int currentDodgeChance;
        if(isEnraged)
        {
            currentDodgeChance = enragedDodgeChance;
        }
        else
        {
            currentDodgeChance = normalDodgeChance;
        }
        
        // don't respond if the Tengu is playing the enraged animation
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
        
        // only respond if player is within range
        if(distance > tenguParryRange)
        {
            return false;
        }
        
        // roll a random num between 0 and 100
        int responseRoll = Random.Range(0,100);
        
        // 0-59 = parry (60% chance)
        if(responseRoll < currentParryChance)
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
            return true;
        }
        // 60-84 = dodge (25% chance)
        else if(responseRoll < currentParryChance + currentDodgeChance)
        {
            // pick a random dodge direction
            int dodgeRoll = Random.Range(3,5);
            dodgeTarget = GetDodgeTarget(dodgeRoll);;
            dodgeStarted = false;
            currentState = TenguState.Dodge;
            return true;
        }

        // 85-99 = gets hit (15% chance)
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
    // DODGE LOGIC
    // -------------------------

    // calculates the world position the Tengu should move to when repositioning
    private Vector3 GetDodgeTarget(int roll)
    {

        // if the roll was a 3 (back up)
        if(roll == 3)
        {
            // calculates the direction that's directly away from the player
            Vector3 dirAway = (transform.position - player.position).normalized;

            // move a certain distance away from the current position
            Vector3 target = transform.position + dirAway * dodgeDistance;

            // keep the same Y position so the Tengu doesn't go into the ground or air
            target.y = transform.position.y;

            return target;
        }
        // if the roll was a 4 (strafe left/right)
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
            Vector3 target = transform.position + sideways * dodgeDistance;

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
        
        // if already enraged, return so that the Tengu can't enrage multiple times
        if(isEnraged)
        {
            return;
        }

        // flag that Tengu is enraged so this can't trigger again
        isEnraged = true; 

        // stop any coroutines so they can't interfere with the enrage animation
        StopAllCoroutines();

        // boost all stats by their corresponding enraged multipliers
        moveSpeed *= enragedSpeedMultiplier;
        timeBetweenAttacks /= enragedAttackSpeedMultiplier;
        dodgeSpeed *= enragedDodgeSpeedMultiplier;

        // play animation, sfx, and vfx
        animator.SetTrigger("Enraged");
        sfx.PlayEnragedSFX();
        vfx.PlayMagicCircleEffect(transform.position, transform);
        vfx.PlayRedCloudEffect(transform.position, transform);

        // start the screen shake for the duration of the enraged animation
        StartCoroutine(ShakeDuringEnraged(7f));

        // switch to enraged state so HandleEnraged() runs every frame
        currentState = TenguState.Enraged;

    }
    


    
    // continuously shakes the screen for the duration of the enrage animation by firing multiple impulses in succession
    private IEnumerator ShakeDuringEnraged(float duration)
    {
        // elapsed time
        float elapsed = 0f;

        // keep shaking until the full duration of the animation has finished playing
        while(elapsed < duration)
        {
            // generate a random direction for the shake each time
            // multiplying by 2f controls how violent the shake is
            Vector3 randomVelocity = new Vector3
            (
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0           
            ) * 2f; 

            // fire the impulse with the random velocity
            impulseSource.GenerateImpulseWithVelocity(randomVelocity);

            // add 0.3s to elapsed and wait 0.3s before firing again
            elapsed += 0.3f;
            yield return new WaitForSeconds(0.3f);
        }
    }




   

   private IEnumerator DashSlashSequence()
    {
        isDashSlashing = true;
        animator.SetTrigger("DashSlash");

        // Sync with the Animator
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("DashSlash"));

        // WIND-UP: Play naturally until the "Lunge" starts (e.g., 30% of animation)
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            yield return null;
        }

        // Freeze the animation on the "dashing" frame
        animator.speed = 0; 

        // Move until we are within attack range
        while (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, dashSlashSpeed * Time.deltaTime);
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            yield return new WaitForFixedUpdate();
        }

        // Resume the animation to play the actual slash
        // We set it back to 1.0 (or 2.2 if you want the swing itself to stay fast)
        animator.speed = 1.0f; 

        // Wait for the animation to finish so we don't snap into a walk mid-swing
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);

        isDashSlashing = false;
        currentState = TenguState.Attack;
    }





    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
