using UnityEngine;
using System.Collections;

public class ToriiGateTrigger : MonoBehaviour
{
    public GameObject[] paradeObjects;
    public float fadeDuration = 2f;
    public string playerTag = "Player";
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioClip paradeMusic;

    public SpawnManager spawnManager;    // Drag your SpawnManager object here

    private bool _triggered = false;
    [SerializeField]private GameObject lightStop;

    void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag(playerTag)) return;
        _triggered = true;

        lightStop.SetActive(false);

        backgroundMusic.Stop();
        backgroundMusic.clip = paradeMusic;
        backgroundMusic.Play();

        // Kick off the first wave
        if (spawnManager != null)
            spawnManager.TriggerFirstWave();

        foreach (GameObject obj in paradeObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                StartCoroutine(FadeIn(obj.GetComponent<Renderer>().material));
            }
        }
    }

    IEnumerator FadeIn(Material mat)
    {
        Color c = mat.color;
        c.a = 0f;
        mat.color = c;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            mat.color = c;
            yield return null;
        }
    }

    public void ResetTrigger()
    {
        _triggered = false;
        GetComponent<Collider>().enabled = true;

        // Stop parade music and fade out parade objects
        foreach (GameObject obj in paradeObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}