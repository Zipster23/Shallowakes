using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SusanooRespawnManager : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private ScreenFade screenFade;
    [SerializeField] private SusanooIntroCinematic susanooCinematic;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private Enemy susanooEnemy;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private SusanooSFXManager sfx;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private SusanooCinematicTrigger cinematicTrigger;
    [SerializeField] private GameObject arenaBarrier;
    [SerializeField] private SusanooSection1 section1;
    [SerializeField] private GameObject land;
    [SerializeField] private SeaSplitCinematic seaSplitCinematic;
    [SerializeField] private Collider seaSplitTrigger;
    [SerializeField] private Collider section1CompleteTrigger;

    [Header("Spawn Positions")]
    [SerializeField] private Vector3 playerSpawnPositionSection1 = new Vector3(-1115.28f, 527.93f, -167.02f);
    [SerializeField] private Vector3 playerSpawnPositionSection2 = new Vector3(-1115.28f, 527.93f, -167.02f);
    [SerializeField] private Vector3 susanooSpawnPosition = new Vector3(-1101.99f, 529f, -223.05f);

    private bool isRespawning = false;
    public bool hasPassedSection1 = false;




    // called by PlayerHealth when the player dies
    public void OnPlayerDied()
    {
        
        if(!isRespawning)
        {
            StartCoroutine(DeathSequence());
        }

    }




    private IEnumerator DeathSequence()
    {

        isRespawning = true;

        musicSource.Stop();
        seaSplitCinematic.musicSource.Stop();
        section1.EndSection1();

        yield return new WaitForSeconds(0.75f);

        sfx.PlayerPlayerDeathSFX();

        yield return new WaitForSeconds(1.5f);

        // fade to black
        yield return StartCoroutine(screenFade.FadeOut());

        // show death screen
        deathScreen.SetActive(true);

        yield return new WaitForSeconds(1f);

        // wait for player to press R
        while(!Input.GetKeyDown(KeyCode.R))
        {
            yield return null;
        }

        // hide death screen
        deathScreen.SetActive(false);

        // reset everything
        ResetEverything();

        // fade back in
        yield return StartCoroutine(screenFade.FadeIn());

        isRespawning = false;

    }




    private void ResetEverything()
    {
        
        // re-enable the cinematic trigger so it can fire again on respawn
        cinematicTrigger.GetComponent<Collider>().enabled = true;

        // reset player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.currentHealth = playerHealth.maxHealth;
        playerHealth.enabled = true;
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<PlayerParry>().enabled = true;
        player.GetComponent<Animator>().SetBool("IsDead", false);
        player.GetComponent<Animator>().Play("Idle", 0, 0f);
        player.GetComponent<PlayerController>().ResetAttack();

        // reset susanoo
        susanooAI.transform.position = susanooSpawnPosition;
        susanooEnemy.currentHealth = susanooEnemy.maxHealth;
        susanooEnemy.enabled = true;
        susanooAI.isEnraged = false;
        susanooAI.GetComponent<Animator>().SetBool("IsDead", false);
        susanooAI.GetComponent<Animator>().Play("Idle", 0, 0f);
        susanooAI.enabled = true;
        susanooAI.currentState = SusanooAI.SusanooState.Idle;
        susanooAI.isAttackActive = false;
        susanooAI.isAttacking = false;

        if(hasPassedSection1)
        {
            // respawn at arena entrance
            player.transform.position = playerSpawnPositionSection2;

            // barrier should already be active from section 1 completion
            arenaBarrier.SetActive(true);
        }
        else
        {
            // respawn at hill
            player.transform.position = playerSpawnPositionSection1;

            // reset sea split
            seaSplitTrigger.enabled = true;

            // reset section 1 complete trigger
            section1CompleteTrigger.enabled = true;

            // stop section 1 if it was running
            section1.EndSection1();

            // reset arena barrier
            arenaBarrier.SetActive(false);

            // reset seasplit cinematic
            seaSplitCinematic.ResetCinematic();
        }

        

    }

}
