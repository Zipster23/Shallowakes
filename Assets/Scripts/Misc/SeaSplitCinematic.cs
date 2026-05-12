using System.Collections;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    [SerializeField] private GameObject land;
    [SerializeField] private WaveSystem waveManager;

    [Header("Platform Movement")]
    [SerializeField] public float platformVelocity = 5f;
    [SerializeField] public float moveDuration = 5f;

    [Header("Particle Settings")]
    [SerializeField] private bool useParticles = true;
    [SerializeField] private GameObject seaParticles;

    [Header("References")]
    [HideInInspector] [SerializeField] public AudioSource musicSource;
    [SerializeField] private AudioClip section1Music;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private SusanooSFXManager sfx;
    [SerializeField] private SusanooSection1 section1;
    [SerializeField] public GameObject windSlashVerticalPrefab;

    private Vector3 _initialPlatformPosition;
    private Coroutine _moveCoroutine;
    private Coroutine _particleCoroutine;

    private void Awake()
    {
        // Store the starting position so we can return to it later
        if (land != null)
        {
            _initialPlatformPosition = land.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerCinematic(other.transform);
        }
    }

    private void TriggerCinematic(Transform playerTransform)
    {
        // 1. Fire huge vertical wind slash
        Vector3 spawnPos = susanooAI.transform.position + susanooAI.transform.forward * 2f;
        Vector3 direction = (playerTransform.position - spawnPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject slash = Instantiate(windSlashVerticalPrefab, spawnPos, rotation);
        WindSlash windSlash = slash.GetComponent<WindSlash>();
        if (windSlash != null)
        {
            windSlash.SetDirection(direction);
            windSlash.speed = 220f;
            windSlash.isParriable = false;
        }

        // 2. Start Platform Movement
        if (land != null)
        {
            _moveCoroutine = StartCoroutine(MovePlatform());
        }

        // 3. Split sea & Particles
        if (waveManager != null)
        {
            waveManager.TriggerAllScaleIn();

            if (useParticles && seaParticles != null)
            {
                _particleCoroutine = StartCoroutine(HandleParticleDuration());
            }
        }

        // 4. Audio
        sfx.PlayWindSlashSFX();
        musicSource.clip = section1Music;
        musicSource.Play();

        // 5. Start section logic
        section1.StartSection1();

        // Disable the trigger so it doesn't fire again until reset
        GetComponent<Collider>().enabled = false;
    }

    private IEnumerator MovePlatform()
    {
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            land.transform.Translate(Vector3.back * platformVelocity * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Sequence complete
        _moveCoroutine = null;
    }

    private IEnumerator HandleParticleDuration()
    {
        seaParticles.SetActive(true);

        // Wait for the movement to finish
        yield return new WaitForSeconds(moveDuration);

        seaParticles.SetActive(false);
        _particleCoroutine = null;
    }

    /// <summary>
    /// Resets the cinematic state for player respawn.
    /// Call this from your Respawn Manager.
    /// </summary>
    public void ResetCinematic()
    {
        // Stop any active movement or particle timers
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        if (_particleCoroutine != null) StopCoroutine(_particleCoroutine);

        // Reset Platform Position
        if (land != null)
        {
            land.transform.position = _initialPlatformPosition;
        }

        // Reset Particles
        if (seaParticles != null)
        {
            seaParticles.SetActive(false);
        }

        // Re-enable the trigger and the GameObject
        GetComponent<Collider>().enabled = true;
        gameObject.SetActive(true);

        // Optional: Reset WaveManager state if it has a Reset method
        waveManager.ResetWaves(); 
    }
}