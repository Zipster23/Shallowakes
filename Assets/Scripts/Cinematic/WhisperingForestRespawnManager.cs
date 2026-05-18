using System.Collections;
using UnityEngine;

public class WhisperingForestRespawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScreenFade screenFade;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private ToriiGateTrigger toriiGateTrigger;
    [SerializeField] private SpawnManager spawnManager;

    [Header("Spawn Positions")]
    [SerializeField] private Vector3 playerSpawnPosition; // set in Inspector, outside the torii gate

    private bool isRespawning = false;

    public void OnPlayerDied()
    {
        if (!isRespawning)
            StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        isRespawning = true;

        musicSource.Stop();
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(screenFade.FadeOut());

        deathScreen.SetActive(true);

        yield return new WaitForSeconds(1f);

        while (!Input.GetKeyDown(KeyCode.R))
            yield return null;

        deathScreen.SetActive(false);

        ResetEverything();

        yield return StartCoroutine(screenFade.FadeIn());

        isRespawning = false;
    }

    private void ResetEverything()
    {
        // Reset player position to outside the torii gate
        player.transform.position = playerSpawnPosition;

        // Reset player health
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.currentHealth = playerHealth.maxHealth;
        playerHealth.enabled = true;

        // Re-enable player scripts
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<PlayerParry>().enabled = true;

        // Reset player animations
        player.GetComponent<Animator>().SetBool("IsDead", false);
        player.GetComponent<Animator>().Play("Idle", 0, 0f);

        // Clean up enemies and reset waves
        spawnManager.CleanUpAndReset();

        // Re-arm the torii gate trigger
        toriiGateTrigger.ResetTrigger();
    }
}