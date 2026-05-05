using System.Collections;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    [SerializeField] private GameObject land;
    [SerializeField] private WaveManager waveManager;

    [Header("Platform Movement")]
    [SerializeField] public float platformVelocity = 5f; // Speed of movement
    [SerializeField] public float moveDuration = 5f;    // How long it moves

    [HideInInspector]
    [SerializeField] public AudioSource musicSource;
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

            // 2. Start Platform Movement
            if (land != null)
            {
                StartCoroutine(MovePlatform());
            }

            // 3. Split sea
            if (waveManager != null)
            {
                waveManager.TriggerAllScaleIn();
            }

            // 4. Play section 1 music
            sfx.PlayWindSlashSFX();
            musicSource.clip = section1Music;
            musicSource.Play();

            // 5. Start section 1 - susanoo throws slashes
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
            // Moves the land in the Positive Z direction (Vector3.forward)
            land.transform.Translate(Vector3.back * platformVelocity * Time.deltaTime, Space.World);

            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }
    }
}