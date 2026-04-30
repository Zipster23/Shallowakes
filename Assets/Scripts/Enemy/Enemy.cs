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

    // --- ENRAGE SETTINGS --- //

    [Header("Enrage")]
    // If true, this enemy has an enraged phase that triggers at the health threshold
    public bool hasEnragedPhase = false;
    // Health value at which the enraged phase triggers (e.g. 20 = triggers at 20hp)
    public int enrageHealthThreshold = 20;


    // --- REFERENCES --- //

    // Cached on Start — whichever AI is present on this GameObject
    private TenguAI tenguAI;
    private YokaiAI yokaiAI;
    private SusanooAI susanooAI;


    void Start()
    {
        currentHealth = maxHealth;

        // Cache whichever AI script is present — only one should exist per enemy
        tenguAI = GetComponent<TenguAI>();
        yokaiAI = GetComponent<YokaiAI>();
        susanooAI = GetComponent<SusanooAI>();
    }


    public void TakeDamage(int damage)
    {
        // Can't be hit when already dead
        if(currentHealth <= 0)
        {
            return;
        }

        // Can't be damaged during Tengu's enraged animation
        if(tenguAI != null && tenguAI.currentState == TenguAI.TenguState.Enraged)
        {
            return;
        }

        // Can't be damaged during Susanoo's enraged animation
        if(susanooAI != null && susanooAI.currentState == SusanooAI.SusanooState.Enraged)
        {
            return;
        }

        // Can't be damaged while a Yokai ability that grants i-frames is active
        // (YokaiAbility scripts can set this flag on YokaiAI if needed)
        if(yokaiAI != null && yokaiAI.currentState == YokaiAI.YokaiState.Ability)
        {
            // Only block damage if the active ability requests invincibility
            // Default: Yokai CAN be hit during abilities unless a script sets this
            // Remove this block if you never want ability i-frames
        }

        currentHealth -= damage;

        // Play hurt animation
        enemyAnimator.SetTrigger("Hurt");

        // Check if enraged phase should trigger
        if(hasEnragedPhase && currentHealth <= enrageHealthThreshold)
        {
            if(tenguAI != null)
            {
                tenguAI.EnterEnragedMode();
            }

            if(susanooAI != null)
            {
                susanooAI.EnterEnragedMode();
            }

            // When you build a YokaiEnraged ability, call it here:
            // yokaiAI?.GetComponent<YokaiEnraged>()?.TryEnrage();
        }

        if(currentHealth <= 0)
        {
            Die();
        }
    }


    public void Die()
    {
        // Play death animation
        enemyAnimator.SetBool("IsDead", true);

        // Disable whichever AI is present
        if(tenguAI != null)
        {
            tenguAI.enabled = false;
            StartCoroutine(TransitionToSusanooStage());
        }

        if(yokaiAI != null)
        {
            yokaiAI.enabled = false;
        }

        if(susanooAI != null)
        {
            susanooAI.enabled = false;
        }

        // Disable this script last
        this.enabled = false;
    }




    private IEnumerator TransitionToSusanooStage()
    {
        
        tenguIntroCinematic.musicSource.Stop();

        // wait for death animation to finish
        yield return new WaitForSeconds(5f);

        // fade to black
        ScreenFade fader = FindObjectOfType<ScreenFade>();
        yield return StartCoroutine(fader.FadeOut());

        // show next stage title
        fader.SetTitleText("Okinoshima Island - The Door to Another Dimension");
        yield return StartCoroutine(fader.ShowTitle());

        // Load next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Susanoo_Domain");

    }
}
