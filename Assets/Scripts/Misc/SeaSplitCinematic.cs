using System.Collections;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    [SerializeField] private GameObject land;
    [SerializeField] private WaveManager waveManager;

    [Header("Platform Movement")]
    [SerializeField] public float platformVelocity = 5f;
    [SerializeField] public float moveDuration = 5f;

    [Header("Particle Settings")]
    [SerializeField] private bool useParticles = true; // The toggle
    [SerializeField] private GameObject seaParticles;   // The assigned GameObject

    [Header("References")]
    [HideInInspector][SerializeField] public AudioSource musicSource;
    [SerializeField] private AudioClip section1Music;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private SusanooSFXManager sfx;
    [SerializeField] private SusanooSection1 section1;
    [SerializeField] public GameObject windSlashVerticalPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Fire huge vertical wind slash
            Vector3 spawnPos = susanooAI.transform.position + susanooAI.transform.forward * 2f;
            Vector3 direction = (other.transform.position - spawnPos).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject slash = Instantiate(windSlashVerticalPrefab, spawnPos, rotation);
            WindSlash windSlash = slash.GetComponent<WindSlash>();
            if (windSlash != null)
            {
                windSlash.SetDirection(direction);
                windSlash.speed = 220f;
                windSlash.isParriable = false;
            }

            // 2 & 7. Start Platform Movement and Particles
            if (land != null)
            {
                StartCoroutine(MovePlatform());
            }

            // 3. Split sea
            if (waveManager != null)
            {
                waveManager.TriggerAllScaleIn();

                // Handle Particles at the same time the wave animation starts
                if (useParticles && seaParticles != null)
                {
                    StartCoroutine(HandleParticleDuration());
                }
            }

            // 4. Play section 1 music
            sfx.PlayWindSlashSFX();
            musicSource.clip = section1Music;
            musicSource.Play();

            // 5. Start section 1
            section1.StartSection1();

            // 6. Disable this trigger
            GetComponent<Collider>().enabled = false;
        }
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
    }

    // New Coroutine to handle the particles
    private IEnumerator HandleParticleDuration()
    {
        seaParticles.SetActive(true);

        // Wait for the same duration as the platform movement/wave split
        yield return new WaitForSeconds(moveDuration);

        seaParticles.SetActive(false);
    }
}