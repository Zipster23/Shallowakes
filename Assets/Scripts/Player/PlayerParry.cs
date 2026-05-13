using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParry : MonoBehaviour
{

    // --- REFERENCES --- //

    [Header("References")]
    public TenguAI tenguAI;
    public SusanooAI susanooAI;
    public List<YokaiAI> yokaiAIs;
    private PlayerInputHandler inputHandler;
    private Animator animator;
    private PlayerVFXManager vfx;
    private PlayerSFXManager sfx;


    // --- PARRY SETTINGS --- //

    [Header("Parry Settings")]
    public float parryWindow = 0.2f;
    public float parryCooldown = 1f;
    public float parryRange = 4f;

    private bool isParrying = false;
    private float parryCooldownTimer = 0f;




    // --- SETUP --- //

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        animator = GetComponent<Animator>();
        vfx = GetComponent<PlayerVFXManager>();
        sfx = GetComponent<PlayerSFXManager>();
    }




    // --- MAIN LOOP --- //

    private void Update()
    {
        parryCooldownTimer -= Time.deltaTime;

        if (inputHandler.parryInput && parryCooldownTimer <= 0f && !isParrying)
        {
            animator.SetTrigger("Parry");
            StartCoroutine(ParryWindow());
        }

        if (isParrying && tenguAI != null)
        {
            float distance = Vector3.Distance(transform.position, tenguAI.transform.position);
            if (tenguAI.isAttackActive && distance <= parryRange)
            {
                SuccessfulParry();
            }
        }

        if (isParrying && susanooAI != null)
        {
            float distance = Vector3.Distance(transform.position, susanooAI.transform.position);
            if (susanooAI.isAttackActive && distance <= parryRange)
            {
                SuccessfulParry();
            }
        }

        if (isParrying)
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, parryRange);
            foreach (Collider col in nearbyEnemies)
            {
                YokaiAI yokai = col.GetComponent<YokaiAI>();
                if (yokai != null && yokai.isAttackActive)
                {
                    SuccessfulParry();
                    break;
                }

                if (col.CompareTag("TutorialDummy"))
                {
                    YokaiAI dummyYokaiAI = col.GetComponent<YokaiAI>();
                    TutorialDummy dummy = col.GetComponent<TutorialDummy>();

                    if (dummy != null && dummyYokaiAI != null && dummyYokaiAI.isAttackActive)
                    {
                        SuccessfulParryOnDummy(dummy);
                        break;
                    }
                }
            }
        }

        // check for wind slash projectiles in parry range
        if (isParrying)
        {
            Collider[] nearbyProjectiles = Physics.OverlapSphere(transform.position, parryRange * 2f);
            foreach (Collider col in nearbyProjectiles)
            {
                WindSlash windSlash = col.GetComponent<WindSlash>();
                if (windSlash != null && windSlash.isParriable)
                {
                    vfx.EmitParryParticles();
                    sfx.playKatanaDeflectSFX();
                    windSlash.GetParried();
                    isParrying = false;
                    parryCooldownTimer = parryCooldown;
                    return;
                }
            }
        }
    }




    // --- PARRY LOGIC --- //

    private IEnumerator ParryWindow()
    {
        isParrying = true;
        yield return new WaitForSeconds(parryWindow);
        isParrying = false;
        parryCooldownTimer = parryCooldown;
    }


    private void SuccessfulParry()
    {
        isParrying = false;
        parryCooldownTimer = parryCooldown;

        vfx.EmitParryParticles();
        sfx.playKatanaDeflectSFX();

        GetComponent<PlayerMovement>().Knockback(6f, 0.2f);

        if (tenguAI != null && tenguAI.isAttackActive)
        {
            if (tenguAI.isDoingCombo)
                tenguAI.comboSlashParried = true;
            else
                tenguAI.GetParried();
        }

        if (susanooAI != null && susanooAI.isAttackActive)
        {
            susanooAI.GetParried();
        }

        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, parryRange);
        foreach (Collider col in nearbyEnemies)
        {
            TutorialDummy dummy = GameObject.FindGameObjectWithTag("TutorialDummy").GetComponent<TutorialDummy>();

            if (dummy != null)
            {
                dummy.OnParried();
            }

            YokaiAI yokai = col.GetComponent<YokaiAI>();
            if (yokai != null && yokai.isAttackActive)
            {
                yokai.GetParried();
                break;
            }
        }
    }

    // ADD THIS: Special parry for tutorial dummy (no knockback, just feedback)
    private void SuccessfulParryOnDummy(TutorialDummy dummy)
    {
        isParrying = false;
        parryCooldownTimer = parryCooldown;

        vfx.EmitParryParticles();
        sfx.playKatanaDeflectSFX();

        // No knockback during tutorial
        dummy.OnParried();
    }

}