using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public Animator enemyAnimator;
    public int maxHealth = 100;
    public int currentHealth;
    [SerializeField] private TenguIntroCinematic tenguIntroCinematic;
    [SerializeField] private SusanooIntroCinematic susanooIntroCinematic;
    [SerializeField] private GameObject mapImage;
    [SerializeField] private YokaiVFXManager yokaiVFXManager;
    [SerializeField] private YokaiSFXManager yokaiSFXManager;

    // --- INVINCIBILITY --- //
    [SerializeField] public bool isInvincible = false;

    // --- ENRAGE SETTINGS --- //

    [Header("Enrage")]
    public bool hasEnragedPhase = false;
    public int enrageHealthThreshold = 20;
    public bool isBoss = false;


    // --- REFERENCES --- //

    private TenguAI tenguAI;
    private YokaiAI yokaiAI;
    private SusanooAI susanooAI;


    void Start()
    {
        currentHealth = maxHealth;

        tenguAI = GetComponent<TenguAI>();
        yokaiAI = GetComponent<YokaiAI>();
        susanooAI = GetComponent<SusanooAI>();

        if (yokaiAI != null)
        {
            yokaiVFXManager = GetComponent<YokaiVFXManager>();
        }
    }


    public void TakeDamage(int damage)
    {
        // ADD THIS: Check invincibility first
        if (isInvincible)
        {
            return;
        }

        // Can't be hit when already dead
        if (currentHealth <= 0)
        {
            return;
        }

        // Can't be damaged during Tengu's enraged animation
        if (tenguAI != null && tenguAI.currentState == TenguAI.TenguState.Enraged)
        {
            return;
        }

        // Can't be damaged during Susanoo's enraged animation
        if (susanooAI != null && susanooAI.currentState == SusanooAI.SusanooState.Enraged)
        {
            return;
        }

        // Can't be damaged while a Yokai ability that grants i-frames is active
        if (yokaiAI != null && yokaiAI.currentState == YokaiAI.YokaiState.Ability)
        {
            // Only block damage if the active ability requests invincibility
        }

        currentHealth -= damage;

        // Play hurt animation
        enemyAnimator.SetTrigger("Hurt");

        // Check if enraged phase should trigger
        if (hasEnragedPhase && currentHealth <= enrageHealthThreshold)
        {
            if (tenguAI != null)
            {
                tenguAI.EnterEnragedMode();
            }

            if (susanooAI != null)
            {
                susanooAI.EnterEnragedMode();
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    public void Die()
    {
        enemyAnimator.SetBool("IsDead", true);

        YokaiAI yokai = GetComponent<YokaiAI>();

        if (isBoss)
        {
            StartCoroutine(BossDefeated());
        }
        else if (yokai != null)
        {
            yokai.InterruptAllAbilities();
            yokaiVFXManager.PlayDeathEffect(transform.position, null);
            yokaiSFXManager.PlayDeathSFX();
            GameObject.Destroy(gameObject);
        }

        this.enabled = false;
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private IEnumerator BossDefeated()
    {
        yield return new WaitForSeconds(5f);

        TenguAI tengu = GetComponent<TenguAI>();
        SusanooAI susanoo = GetComponent<SusanooAI>();
        YokaiAI yokai = GetComponent<YokaiAI>();

        if (tengu != null)
        {
            StageProgress.CompleteTenguShrine();
            PlayerPrefs.SetInt("ReturnToMap", 1);
            PlayerPrefs.Save();
            ScreenFade fade = FindObjectOfType<ScreenFade>();
            if (fade != null) yield return StartCoroutine(fade.FadeOut());
            SceneManager.LoadScene("Main_Menu");
        }
        else if (susanoo != null)
        {
            SusanooDeathSequence deathSequence = FindObjectOfType<SusanooDeathSequence>();
            if (deathSequence != null) deathSequence.StartDeathEnding();
        }
        else if (yokai != null)
        {
            StageProgress.CompleteWhisperingForest();
            PlayerPrefs.SetInt("ReturnToMap", 1);
            PlayerPrefs.Save();
            ScreenFade fade = FindObjectOfType<ScreenFade>();
            if (fade != null) yield return StartCoroutine(fade.FadeOut());
            SceneManager.LoadScene("Main_Menu");
        }
    }
}