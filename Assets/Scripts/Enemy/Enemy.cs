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

    // --- ENRAGE SETTINGS --- //

    [Header("Enrage")]
    // If true, this enemy has an enraged phase that triggers at the health threshold
    public bool hasEnragedPhase = false;
    // Health value at which the enraged phase triggers (e.g. 20 = triggers at 20hp)
    public int enrageHealthThreshold = 20;
    // check this in the inspector for boss enemies. Used to send player back to the map picture so they can move from stage to stage
    public bool isBoss = false;


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

        // Disable this script last
        this.enabled = false;

        if(isBoss)
        {
            StartCoroutine(BossDefeated());
        }
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }




    private IEnumerator BossDefeated()
    {
        
        // wait for death anim to play
        yield return new WaitForSeconds(5f);

        // figure out which boss this is and save progress
        TenguAI tengu = GetComponent<TenguAI>();
        SusanooAI susanoo = GetComponent<SusanooAI>();

        if(tengu != null)
        {
            StageProgress.CompleteTenguShrine();
        }
        else if(susanoo != null)
        {
            StageProgress.CompleteSusanoo();
        }

        // tell main menu to skip straight to map
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();

        // fade to black and then go back to main menu
        ScreenFade fader = FindObjectOfType<ScreenFade>();
        if(fader != null)
        {
            yield return StartCoroutine(fader.FadeOut());
        }

        SceneManager.LoadScene("Main_Menu");

    }




    private IEnumerator SusanooDeathEnding()
    {
        susanooIntroCinematic.musicSource.Stop();

        yield return new WaitForSeconds(5f);

        // fade to black
        ScreenFade fader = FindObjectOfType<ScreenFade>();
        yield return StartCoroutine(fader.FadeOut());

        // show ending cards one by one
        fader.SetTitleText("Susanoo has fallen.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("The storms that plagued the land began to fade.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("Shallo had fulfilled his oath.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("Kamehime was free.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("Thank you for playing.");
        yield return StartCoroutine(fader.ShowTitle());

        // fade out one last time and quit
        yield return StartCoroutine(fader.FadeOut());

        // mark susanoo as complete and go back to map
        StageProgress.CompleteSusanoo();
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");

    }

}
