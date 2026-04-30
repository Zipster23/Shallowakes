using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguRespawnManager : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private ScreenFade screenFade;
    [SerializeField] private TenguIntroCinematic tenguCinematic;
    [SerializeField] private TenguAI tenguAI;
    [SerializeField] private Enemy tenguEnemy;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TenguSFXManager sfx;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private CinematicTrigger cinematicTrigger;
    [SerializeField] private GameObject arenaBarrier;

    [Header("Spawn Positions")]
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-1115.28f, 527.93f, -167.02f);
    [SerializeField] private Vector3 tenguSpawnPosition = new Vector3(-1101.99f, 529f, -223.05f);

    private bool isRespawning = false;




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

        sfx.PlayPlayerDeathSFX();

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

        // reset player position
        player.transform.position = playerSpawnPosition;

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
        tenguAI.transform.position = tenguSpawnPosition;
        tenguEnemy.currentHealth = tenguEnemy.maxHealth;
        tenguEnemy.enabled = true;
        tenguAI.isEnraged = false;

        // reset tengu animator
        tenguAI.GetComponent<Animator>().SetBool("IsDead", false);
        tenguAI.GetComponent<Animator>().Play("Idle", 0, 0f);

        // reset tengu AI state
        tenguAI.enabled = true;
        tenguAI.currentState = TenguAI.TenguState.Idle;
        tenguAI.isAttackActive = false;
        tenguAI.isAttacking = false;

        // deactivate tengu so the arena trigger can reactivate him
        tenguAI.gameObject.SetActive(false);

        arenaBarrier.SetActive(false);

        // since cinematic already played, just trigger opening dash slash directly
        tenguAI.TriggerOpeningDashSlash();

    }

}
