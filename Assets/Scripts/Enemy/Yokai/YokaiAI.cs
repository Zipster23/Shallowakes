using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base AI script for Yokai enemies.
/// Handles core combat: Idle, Chase, Attack, Parry, Dodge, IsParried.
/// 
/// Abilities (ComboAttack, DashSlash, ShadowClone, etc.) are handled by
/// separate YokaiAbility scripts attached to the same GameObject.
/// Abilities communicate with this script via the public API at the bottom of the file.
/// </summary>
public class YokaiAI : MonoBehaviour
{

    // --- STATES --- //

    public enum YokaiState
    {
        Idle,       // standing still, waiting for player to get close
        Chase,      // running towards the player
        Attack,     // in attack range, swinging at the player
        Parry,      // parrying the player's attack
        Dodge,      // backing off or strafing after the player attacks
        IsParried,  // briefly stunned after getting parried by the player
        Ability,    // an external ability script has taken control
    }

    [Header("States")]
    public YokaiState currentState = YokaiState.Idle;


    // --- REFERENCES --- //

    [Header("References")]
    public Transform player;            // reference to the player's transform
    private Animator animator;          // controls the Yokai's animations
    private Enemy enemy;                // reference to the Enemy script (for health)
    //private YokaiVFXManager vfx;        // handles hit visual effects
    //private YokaiSFXManager sfx;        // handles hit sound effects
    private Rigidbody rb;               // handles Yokai's rigidbody


    // --- MOVEMENT --- //

    [Header("Movement")]
    public float chaseRange = 15f;      // how far away the player can be before the Yokai starts chasing
    public float moveSpeed = 10f;       // how fast the Yokai moves

    private Vector3 moveTarget;
    private float currentMoveSpeed;
    private bool isMovingThisFrame;


    // --- ATTACK --- //

    [Header("Attack")]
    public Transform attackPoint;           // empty GameObject positioned in front of Yokai
    public float attackRange = 3f;          // how close the player needs to be for Yokai to attack
    public int attackDamage = 100;          // how much damage each hit deals
    public LayerMask playerLayer;           // used to detect only the player in the attack overlap sphere

    public float timeBetweenAttacks = 1.5f; // how long the Yokai waits between attacks
    [HideInInspector]
    public float attackTimer = 0f;          // counts down to the next attack
    [HideInInspector]
    public bool isAttacking = false;        // prevents the Yokai from moving or switching states mid-attack

    public bool isAttackActive = false;     // true while Yokai is mid-swing, used by parry system to detect if attack can be parried
    public float parryStunDuration = 2f;    // how long the Yokai is stunned after getting parried

    public float yokaiParryRange = 3f;      // how close the player needs to be for the Yokai to parry


    // --- RESPONSE CHANCES --- //

    [Header("Response Chances")]
    public int parryChance = 60;            // chance the Yokai parries the player's attack
    public int dodgeChance = 25;            // chance the Yokai dodges the player's attack
    public float dodgeDistance = 12f;       // how far the Yokai moves when repositioning
    private Vector3 dodgeTarget;            // the position the Yokai is moving towards when repositioning
    public float dodgeSpeed = 40f;          // the speed of the Yokai while repositioning
    private bool dodgeStarted = false;      // prevents VFX and SFX from playing every frame during the dash

    // --- ABILITY HOOK --- //

    // Registered ability scripts call NotifyAbilityComplete() when they finish.
    // This list lets YokaiAI know which abilities are available so it can ask them to trigger.
    private List<YokaiAbility> registeredAbilities = new List<YokaiAbility>();


    // -------------------------
    // SETUP
    // -------------------------

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy    = GetComponent<Enemy>();
        //vfx      = GetComponent<YokaiVFXManager>();
        //sfx      = GetComponent<YokaiSFXManager>();
        rb       = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Auto-register any YokaiAbility scripts on this GameObject
        foreach(YokaiAbility ability in GetComponents<YokaiAbility>())
        {
            RegisterAbility(ability);
        }
    }

    
    // Rigidbody Loop

    private void Update()
    {
        Debug.Log($"Current state: {currentState}");
        if(enemy.currentHealth <= 0) return;
        if(player.GetComponent<PlayerHealth>().currentHealth <= 0)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        switch(currentState)
        {
            case YokaiState.Idle:      HandleIdle();      break;
            case YokaiState.Chase:     HandleChase();     break;
            case YokaiState.Attack:    HandleAttack();    break;
            case YokaiState.Parry:     HandleParry();     break;
            case YokaiState.Dodge:     HandleDodge();     break;
            case YokaiState.IsParried: HandleIsParried(); break;
            case YokaiState.Ability:                      break;
        }
    }

    // -------------------------
    // STATE HANDLERS
    // -------------------------

    private void HandleIdle()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(distanceToPlayer <= chaseRange)
        {
            currentState = YokaiState.Chase;
        }
    }


    private void HandleChase()
    {
        Debug.Log($"Chase - dist: {Vector3.Distance(transform.position, player.position)}, moveSpeed: {moveSpeed}");
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        FacePlayer();

        if(distanceToPlayer > attackRange)
        {
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        if(distanceToPlayer <= attackRange)
        {
            animator.SetBool("IsMoving", false);
            attackTimer = 0f;
            currentState = YokaiState.Attack;
        }
    }


    private void HandleAttack()
    {
        Debug.Log($"isAttacking: {isAttacking}, timer: {attackTimer}, dist: {Vector3.Distance(transform.position, player.position)}");

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        animator.SetBool("IsMoving", false);

        // player ran out of range and Yokai isn't mid-swing
        if(distanceToPlayer > attackRange * 1.5f && !isAttacking)
        {
            currentState = YokaiState.Chase;
            return;
        }

        attackTimer -= Time.deltaTime;

        if(attackTimer <= 0f && !isAttacking)
        {
            // double-check range before attacking
            if(distanceToPlayer > attackRange)
            {
                currentState = YokaiState.Chase;
                return;
            }

            // --- ABILITY HOOK ---
            // Ask each registered ability if it wants to trigger instead of a basic attack.
            // The first ability that returns true takes control; Yokai enters Ability state.
            foreach(YokaiAbility ability in registeredAbilities)
            {
                if(ability.TryTrigger(this))
                {
                    isAttacking = true;
                    currentState = YokaiState.Ability;
                    return;
                }
            }

            // No ability triggered — do a basic attack
            animator.SetTrigger("Attack");
            attackTimer = timeBetweenAttacks;
            isAttacking = true;
        }
    }


    private void HandleDodge()
    {
        FacePlayer();

        float distanceToTarget = Vector3.Distance(transform.position, dodgeTarget);

        if(distanceToTarget > 0.5f)
        {
            if(!dodgeStarted)
            {
                dodgeStarted = true;
                //vfx.PlayDodgeEffect(transform.position, transform);
                //sfx.PlayDodgeSFX();
            }

            Vector3 flatDodgeTarget = new Vector3(dodgeTarget.x, transform.position.y, dodgeTarget.z);
            transform.position = Vector3.MoveTowards(transform.position, flatDodgeTarget, dodgeSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            // reached dodge target — go back to chasing
            dodgeStarted = false;
            animator.SetBool("IsMoving", false);
            currentState = YokaiState.Chase;
        }
    }


    private void HandleParry()
    {
        animator.SetBool("IsMoving", false);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsName("Parry") && stateInfo.normalizedTime >= 1f)
        {
            currentState = YokaiState.Idle;
        }
    }


    private void HandleIsParried()
    {
        animator.SetBool("IsMoving", false);
        // stun duration is managed by ParryStun coroutine
    }


    // -------------------------
    // ATTACK HIT DETECTION
    // -------------------------

    // Called by Animation Event at the frame the weapon connects with the player
    public void Attack()
    {
        if(currentState == YokaiState.Dodge)    return;
        if(currentState == YokaiState.IsParried) return;
        if(currentState == YokaiState.Parry)     return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach(Collider playerHit in hitPlayers)
        {
            playerHit.GetComponentInParent<PlayerHealth>().TakeDamage(attackDamage);
            //vfx.PlayHitEffect(playerHit.transform.position + Vector3.up * 2f);
            //sfx.PlayWeaponHitSFX();
        }

        isAttackActive = false;
    }

    // Called by Animation Event at the start of the attack swing — weapon is now parriable
    public void EnableWeaponCollider()
    {
        isAttackActive = true;
    }

    // Called by Animation Event at the end of the attack swing — weapon is no longer parriable
    public void DisableWeaponCollider()
    {
        isAttackActive = false;
    }

    // Called by Animation Event at the end of the attack animation — Yokai can attack again
    public void OnAttackEnd()
    {
        isAttacking = false;
    }


    // -------------------------
    // PARRY & DODGE RESPONSES
    // -------------------------

    // Called by the player's Attack() animation event.
    // Checks if the Yokai decides to parry or dodge the incoming hit.
    // Returns true if the Yokai responded (parried or dodged), false if it takes the hit.
    public bool CheckYokaiResponse()
    {
        // don't respond during ability
        if(currentState == YokaiState.Ability)  return false;

        // don't respond while already parrying
        if(currentState == YokaiState.Parry)    return false;

        float distance = Vector3.Distance(transform.position, player.position);

        // only respond if player is within parry range
        if(distance > yokaiParryRange) return false;

        int responseRoll = Random.Range(0, 100);

        // Parry
        if(responseRoll < parryChance)
        {
            currentState = YokaiState.Parry;
            animator.SetTrigger("Parry");
            //vfx.EmitSparkParticles();
            //sfx.PlayWeaponDeflectSFX();

            attackTimer = 0f; // Yokai can attack immediately after parrying

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            StartCoroutine(playerHealth.GetParried());
            return true;
        }
        // Dodge
        else if(responseRoll < parryChance + dodgeChance)
        {
            int dodgeRoll = Random.Range(3, 5);
            dodgeTarget  = GetDodgeTarget(dodgeRoll);
            dodgeStarted = false;
            currentState = YokaiState.Dodge;
            return true;
        }

        // Takes the hit
        return false;
    }


    // Called by PlayerParry script when the player successfully parries the Yokai's attack
    public void GetParried()
    {
        // Let the active ability handle getting parried if it wants to
        foreach(YokaiAbility ability in registeredAbilities)
        {
            if(ability.OnYokaiParried())
            {
                return; // ability handled it
            }
        }

        // Default parry response
        isAttacking     = false;
        isAttackActive  = false;

        StopAllCoroutines();
        animator.ResetTrigger("Attack");
        animator.Play("Idle");

        StartCoroutine(ParryStun());
    }


    // Stunned for parryStunDuration, then returns to Idle
    private IEnumerator ParryStun()
    {
        currentState = YokaiState.IsParried;
        yield return new WaitForSeconds(parryStunDuration);
        currentState = YokaiState.Idle;
    }


    // Knocks the Yokai backwards — called when the player successfully parries
    public void Knockback(float force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(float force, float duration)
    {
        float elapsed = 0f;
        Vector3 knockbackDirection = -transform.forward;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(force, 0f, elapsed / duration);
            transform.position += knockbackDirection * strength * Time.deltaTime;
            yield return null;
        }
    }


    // -------------------------
    // DODGE LOGIC
    // -------------------------

    private Vector3 GetDodgeTarget(int roll)
    {
        if(roll == 3) // back up
        {
            Vector3 dirAway = (transform.position - player.position).normalized;
            Vector3 target  = transform.position + dirAway * dodgeDistance;
            target.y = transform.position.y;
            return target;
        }
        else // strafe left or right
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            Vector3 sideways    = Vector3.Cross(dirToPlayer, Vector3.up);

            if(Random.Range(0, 2) == 0) sideways = -sideways;

            Vector3 target = transform.position + sideways * dodgeDistance;
            target.y = transform.position.y;
            return target;
        }
    }


    // -------------------------
    // ABILITY SYSTEM API
    // -------------------------

    /// <summary>
    /// Called by each YokaiAbility in Start() to register itself with this AI.
    /// </summary>
    public void RegisterAbility(YokaiAbility ability)
    {
        if(!registeredAbilities.Contains(ability))
        {
            registeredAbilities.Add(ability);
        }
    }

    /// <summary>
    /// Called by a YokaiAbility when it has finished executing.
    /// Returns the Yokai to the Attack state so normal combat resumes.
    /// </summary>
    public void NotifyAbilityComplete()
    {
        isAttacking = false;
        attackTimer = timeBetweenAttacks;
        currentState = YokaiState.Attack;
    }

    /// <summary>
    /// Called by a YokaiAbility when it needs the Yokai to stop all coroutines
    /// (e.g. a higher-priority ability or phase change interrupts it).
    /// </summary>
    public void InterruptAllAbilities()
    {
        StopAllCoroutines();
        foreach(YokaiAbility ability in registeredAbilities)
        {
            ability.OnInterrupted();
        }
    }


    // -------------------------
    // HELPERS
    // -------------------------

    public void FacePlayer()
    {
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }


    // -------------------------
    // DEBUG
    // -------------------------

    public void OnDrawGizmosSelected()
    {
        if(attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
