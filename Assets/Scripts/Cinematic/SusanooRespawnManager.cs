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

        yield return new WaitForSeconds(0.75f);

        sfx.PlayerPlayerDeathSFX();

        yield return new WaitForSeconds(1.5f);

        // fade to black
        yield return StartCoroutine(screenFade.FadeOut());

        // show death screen
        deathScreen.SetActive(true);

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

        // reset player position based on what section they're on
        if(hasPassedSection1)
        {
            player.transform.position = playerSpawnPositionSection2;
        }
        else
        {
            player.transform.position = playerSpawnPositionSection1;
        }

        // reset player health
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.currentHealth = playerHealth.maxHealth;
        playerHealth.enabled = true;

        // re-enable player scripts
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<PlayerParry>().enabled = true;

        // reset player animations
        player.GetComponent<Animator>().SetBool("IsDead", false);
        player.GetComponent<Animator>().Play("Idle", 0, 0f);

        // reset tengu position and health
        susanooAI.transform.position = susanooSpawnPosition;
        susanooEnemy.currentHealth = susanooEnemy.maxHealth;
        susanooEnemy.enabled = true;
        susanooAI.isEnraged = false;

        // reset tengu animator
        susanooAI.GetComponent<Animator>().SetBool("IsDead", false);
        susanooAI.GetComponent<Animator>().Play("Idle", 0, 0f);

        // reset tengu AI state
        susanooAI.enabled = true;
        susanooAI.currentState = SusanooAI.SusanooState.Idle;
        susanooAI.isAttackActive = false;
        susanooAI.isAttacking = false;

        // deactivate tengu so the arena trigger can reactivate him
        susanooAI.gameObject.SetActive(false);

        arenaBarrier.SetActive(false);

    }

}
