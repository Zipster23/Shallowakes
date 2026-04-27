using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class SusanooAI : MonoBehaviour
{
    
    // --- STATES --- // 

    // All possible states the Tengu can be in
    public enum SusanooState
    {
        Idle,               // standing still, waiting for player to get close
        Chase,              // running towards the player
        Attack,             // in attack range, swinging at the player
        Parry,              // parrying the player's attack
        Dodge,              // backing off or strafing after an attack
        Enraged,            // enraged state - faster/stronger
        IsParried,          // briefly stunned after getting parried by the player
        LightningStrike,    
        WindSlash
    }

    [Header("States")]
    // The state the Tengu is currently in (starts in Idle)
    public SusanooState currentState = SusanooState.Idle;


    // --- REFERENCES --- //

    [Header("References")]
    public Transform player;        // reference to the player's transform
    private Animator animator;      // controls the Tengu's animations
    private Enemy enemy;            // reference to the Enemy script (for health)
    private SusanooVFXManager vfx;    // handles hit visual effects
    private SusanooSFXManager sfx;    // handles hit sound effects
    private Rigidbody rb;           // handles Tengu rigidbody (for enraged)
    public LayerMask groundLayer;   // used for lightning strike ground detection


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
    [HideInInspector]
    public float attackTimer = 0f;          // counts down to the next attack
    [HideInInspector]
    public bool isAttacking = false;        // prevents the Tengu from moving or switching states mid-attack

    public bool isAttackActive = false;     // true while Tengu is mid-swing, used by parry system to detect if attack can be parried
    public float parryStunDuration = 2f;    // how long the Tengu is stunned for after getting parried

    public float tenguParryRange = 3f;      // how close the player needs to be for the Tengu to parry


    // --- RESPONSE CHANCES --- //

    [Header("Response Chances")]
    public int normalParryChance = 60;      // parry chance in normal mode
    public int normalDodgeChance = 25;      // dodge chance in normal mode
    public int enragedParryChance = 20;     // parry chance in enraged mode
    public int enragedDodgeChance = 65;     // dodge chance in enraged mode    
    public float dodgeDistance = 12f;       // how far the Tengu moves when repositioning
    private Vector3 dodgeTarget;            // The position the Tengu is moving towards when repositioning
    public float dodgeSpeed = 40f;          // the speed of the tengu after Repositioning
    private bool dodgeStarted = false;      // prevents VFX and SFX from playing every frame during the dash


    // --- ENRAGED --- //

    [Header("Enraged")]
    public float enragedSpeedMultiplier = 1.5f;         // how much faster the Tengu moves while enraged
    public float enragedAttackSpeedMultiplier = 1.5f;   // how much faster the Tengu attacks while enraged
    public float enragedDodgeSpeedMultiplier = 1.5f;    // how much faster the Tengu dashes while enraged
    private bool isEnraged = false;                     // bool to prevent Tengu from enraging multiple times
    public CinemachineImpulseSource impulseSource;      // reference to Cinemachine Impulse Source on MainCamera to generate screen shake


    // --- ABILITIES --- //

    [Header("Lightning-Strike Ability")]
    public GameObject lightningIndicatorPrefab;     // blue circle that shows where the lightning is gonna strike
    public GameObject lightningVFXPrefab;           // the lightning strike VFX
    public int lightningStrikeCount = 8;            // how many lightning strikes spawn
    public float lightningStrikeRadius = 10f;       // how far from the player strikes can spawn
    public float lightningWarningDuration = 1.5f;   // how long indicators show before lightning strikes
    public int lightningChance = 30;                // percentage chance of doing this ability
    private bool isLightningStriking = false;       // prevents ability from restarting every frame

    [Header("Wind-Slash Ability")]
    public GameObject windSlashHorizontalPrefab;    // the wind slash horizontal projectile prefab
    public GameObject windSlashVerticalPrefab;      // the wind slash vertical projectile prefab
    public int windSlashChance = 25;                // percentage chance of doing wind slash
    public float timeBetweenSlashes = 0.5f;         // time between the two slashes
    private bool isWindSlashing = false;            // prevents ability from restarting every frame





    // --- SETUP --- //

    private void Awake()
    {
        // grab all required components on this gameobject
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        vfx = GetComponent<SusanooVFXManager>();
        sfx = GetComponent<SusanooSFXManager>();
        rb = GetComponent<Rigidbody>();
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

        // if enraged animation is playing, only run HandleEnraged and nothing else
        if(currentState == SusanooState.Enraged)
        {
            HandleEnraged();
            return;
        }

        // run whichever state we're currently in
        switch(currentState)
        {

            case SusanooState.Idle:
                HandleIdle();
                break;
            case SusanooState.Chase:
                HandleChase();
                break;
            case SusanooState.Attack:
                HandleAttack();
                break;
            case SusanooState.Parry:
                HandleParry();
                break;
            case SusanooState.Dodge:
                HandleDodge();
                break;
            case SusanooState.IsParried:
                HandleIsParried();
                break;
            case SusanooState.LightningStrike:
                HandleLightningStrike();
                break;
            case SusanooState.WindSlash:
                HandleWindSlash();
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
            currentState = SusanooState.Chase;
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
            currentState = SusanooState.Attack;   // switch to Attack state
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
            currentState = SusanooState.Chase;
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
                currentState = SusanooState.Chase;
                return; 
            }

            // random chance to do lightning strike
            int lightningRoll = Random.Range(0,100);
            if(lightningRoll < lightningChance)
            {
                isAttacking = true;
                currentState = SusanooState.LightningStrike;
                return;
            }

            // random chance to do wind slash
            int windSlashRoll = Random.Range(0,100);
            if(windSlashRoll < windSlashChance)
            {
                isAttacking = true;
                currentState = SusanooState.WindSlash;
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
            currentState = SusanooState.Chase;
            
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
            currentState = SusanooState.Idle;
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
            // reset attack timer so Tengu attacks immediately after enraging
            attackTimer = 0f;
            currentState = SusanooState.Chase;
        }

    }




    private void HandleLightningStrike()
    {
        
        if(!isLightningStriking)
        {
            isLightningStriking = true;
            StartCoroutine(LightningStrikeSequence());
        }

    }




    private void HandleWindSlash()
    {
        
        if(!isWindSlashing)
        {
            isWindSlashing = true;
            StartCoroutine(WindSlashSequence());
        }

    }

    








    // -------------------------
    // ATTACK HIT DETECTION
    // -------------------------

    // this function is called by an Animation Event at the frame the naginata connects with the player 
    public void Attack()
    {
    
        // if the Tengu is repositioning, it can't attack
        if(currentState == SusanooState.Dodge)
        {
            return;
        }

        // don't deal damage to the player if parried
        if(currentState == SusanooState.IsParried)
        {
            return;
        }

        // don't deal damage while parrying
        if(currentState == SusanooState.Parry)
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
            sfx.PlayBladeHitSFX();

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
        player.GetComponent<PlayerParry>().parryCooldown = 1f; // restore normal parry cooldown
        StopAllCoroutines();                // cancel any running reposition coroutines
        animator.ResetTrigger("Attack");    // cancel the attack trigger
        animator.Play("Idle");              // snap back to idle animation 

        StartCoroutine(ParryStun());        // start the stun for getting parried

    }




    // called by the player's Attack() animation event - checks if the Tengu decides to parry or dodge player attack
    // returns true if Tengu parried/dodged, false if not
    public bool CheckSusanooResponse()
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
        if(currentState == SusanooState.Enraged)
        {
            return false;
        }
    
        // dont parry if already parrying
        if(currentState == SusanooState.Parry)
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
            currentState = SusanooState.Parry;

            // play parry animation & visual feedback
            animator.SetTrigger("Parry");
            vfx.EmitSparkParticles();
            sfx.PlayBladeDeflectSFX();

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
            currentState = SusanooState.Dodge;
            return true;
        }

        // 85-99 = gets hit (15% chance)
        return false;

    }




    // called after Tengu gets parried in GetParried(), waits for parryStunDuration seconds, then sends Tengu back to Idle
    private IEnumerator ParryStun()
    {
        
        currentState = SusanooState.IsParried;                    // enter the parried state
        yield return new WaitForSeconds(parryStunDuration);     // wait for stun to finish
        currentState = SusanooState.Idle;                         // go back to Idle state

    }




    // Knocks the Tengu backwards — called when the player successfully parries the Tengu
    public void Knockback(float force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(float force, float duration)
    {
        float elapsed = 0f;

        // Knock back in the opposite direction the Tengu is facing
        Vector3 knockbackDirection = -transform.forward;

        Rigidbody rb = GetComponent<Rigidbody>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Fade the force out so it feels like a stumble rather than a slide
            float strength = Mathf.Lerp(force, 0f, elapsed / duration);
            rb.MovePosition(rb.position + knockbackDirection * strength * Time.deltaTime);

            yield return null;
        }
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

        // reset ALL flags so nothing can interfere with the enraged animation
        isAttacking = false;
        dodgeStarted = false;
        animator.SetFloat("DashSlashSpeed", 1f);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("DashSlash");
        animator.ResetTrigger("ComboAttack");

        // boost all stats by their corresponding enraged multipliers
        moveSpeed *= enragedSpeedMultiplier;
        timeBetweenAttacks /= enragedAttackSpeedMultiplier;
        dodgeSpeed *= enragedDodgeSpeedMultiplier;

        // play animation, sfx, and vfx
        animator.SetTrigger("Enraged");

        // knock the player back away from the tengu
        Vector3 knockbackDir = (player.position - transform.position).normalized;
        Vector3 knockbackForce = knockbackDir * 15f + Vector3.up * 8f;
        player.GetComponent<PlayerMovement>().StartCoroutine(player.GetComponent<PlayerMovement>().ApplyKnockback(knockbackForce, 0.5f));

        // start the screen shake for the duration of the enraged animation
        StartCoroutine(ShakeDuringEnraged(7f));

        // switch to enraged state so HandleEnraged() runs every frame
        currentState = SusanooState.Enraged;

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




    // -------------------------
    // LIGHTNING STRIKE LOGIC
    // -------------------------

    private IEnumerator LightningStrikeSequence()
    {
        
        // stop moving during ability
        animator.SetBool("IsMoving", false);

        // dash backwards before ability
        Vector3 dashBackTarget = GetDodgeTarget(3); // 3 = dash back
        float elapsed = 0f;
        float dashDuration = 0.3f;
        Vector3 startPos = transform.position;
        sfx.PlayDodgeSFX();
        vfx.PlayDodgeEffect(transform.position, transform);
        while(elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, dashBackTarget, elapsed / dashDuration);
            yield return null;
        }

        // store all spawned indicators so we can destroy them later
        List<GameObject> indicators = new List<GameObject>();

        animator.SetTrigger("LightningStrike");
        vfx.PlayDodgeEffect(transform.position, transform);

        // spawn one indicator directly on the player so they're forced to move
        Vector3 playerIndicatorPos = new Vector3(player.position.x, GetGroundHeight(player.position), player.position.z);
        GameObject playerIndicator = Instantiate(lightningIndicatorPrefab, playerIndicatorPos, Quaternion.identity);
        indicators.Add(playerIndicator);

        // spawn remaining indicators at random positions around the player
        for(int i = 0; i < lightningStrikeCount - 1; i++)
        {
            
            // pick a random position within the strike radius
            Vector2 randomCircle = Random.insideUnitCircle * lightningStrikeRadius;
            Vector3 randomPos = new Vector3
            (
                player.position.x + randomCircle.x,
                player.position.y + 0.1f,
                player.position.z + randomCircle.y
            );

            GameObject indicator = Instantiate(lightningIndicatorPrefab, randomPos, Quaternion.identity);
            indicators.Add(indicator);

        }

        // wait for warning duration so player can move out of the way
        yield return new WaitForSeconds(lightningWarningDuration);

        // strike all indicator positions simultaneously
        foreach(GameObject indicator in indicators)
        {
            
            // spawn lightning vfx at indicator position
            GameObject lightning = Instantiate(lightningVFXPrefab, indicator.transform.position, Quaternion.identity);
            Destroy(lightning, 2f);

            // check if player is standing in this indicator
            float distanceToPlayer = Vector3.Distance(indicator.transform.position, player.position);
            if(distanceToPlayer <= 3.3f)
            {
                // player got hit by lightning
                vfx.PlayHitEffect(player.transform.position + Vector3.up * 2f);
                sfx.PlayBladeHitSFX();
                player.GetComponent<PlayerHealth>().TakeDamage(999);
            }

            // destroy the indicator
            Destroy(indicator);

        }
        sfx.PlayLightningStrikeSFX();
        // wait for lightning VFX to finish
        yield return new WaitForSeconds(1f);

        // reset and go back to attack
        isLightningStriking = false;
        isAttacking = false;
        currentState = SusanooState.Chase;

    }




    // finds the ground height at a given position using raycast
    private float GetGroundHeight(Vector3 position)
    {
        
        RaycastHit hit;

        if(Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, groundLayer))
        {
            return hit.point.y;
        }

        return player.position.y;

    }




    // -------------------------
    // WIND SLASH LOGIC
    // -------------------------

    private IEnumerator WindSlashSequence()
    {
        
        // stop moving during ability & play wind slash animation
        animator.SetBool("IsMoving", false);
        animator.SetTrigger("WindSlash");

        // dash backwards
        Vector3 dashBackTarget = GetDodgeTarget(3); // 3 = back up
        float elapsed = 0f;
        float dashDuration = 0.3f;
        Vector3 startPos = transform.position;
        sfx.PlayDodgeSFX();
        vfx.PlayDodgeEffect(transform.position, transform);
        while(elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, dashBackTarget, elapsed / dashDuration);
            yield return null;
        }

        // face the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // wait to time it with the animation
        yield return new WaitForSeconds(0.7f);

        // fire vertical slash first - player must parry
        sfx.PlayWindSlashSFX();
        FireWindSlash(WindSlash.SlashType.Vertical, 1f);

        // wait before firing second slash
        yield return new WaitForSeconds(timeBetweenSlashes);

        // fire big horizontal slash - player must jump
        sfx.PlayWindSlashSFX();
        FireWindSlash(WindSlash.SlashType.Horizontal, 3f);

        // wait for slashes to travel
        yield return new WaitForSeconds(1.5f);

        // reset and go back to attack
        isWindSlashing = false;
        isAttacking = false;
        currentState = SusanooState.Chase;

    }




    private void FireWindSlash(WindSlash.SlashType slashType, float scale)
    {
        
        Vector3 spawnPos = transform.position + transform.forward;
        Vector3 direction = (player.position - spawnPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // use vertical prefab for vertical slash
        GameObject prefabToUse = slashType == WindSlash.SlashType.Vertical ? windSlashVerticalPrefab : windSlashHorizontalPrefab;

        GameObject slash = Instantiate(prefabToUse, spawnPos, rotation);
        slash.transform.localScale = Vector3.one * scale;

        WindSlash windSlash = slash.GetComponent<WindSlash>();
        if(windSlash != null)
        {
            windSlash.SetDirection(direction);
            windSlash.slashType = slashType;

            if(slashType == WindSlash.SlashType.Horizontal)
            {
                windSlash.isParriable = false;
                Vector3 pos = slash.transform.position;
                pos.y = player.position.y;
                slash.transform.position = pos;
            }

            if(slashType == WindSlash.SlashType.Vertical)
            {
                Vector3 pos = slash.transform.position;
                pos.y = player.position.y + 2f;
                slash.transform.position = pos;
            }
        }

    }







    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
