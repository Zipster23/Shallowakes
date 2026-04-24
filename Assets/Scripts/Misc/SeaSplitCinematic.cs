using System.Collections;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    [Header("Water Halves")]
    public Transform waterLeft;
    public Transform waterRight;

    [Header("Wave Halves")]
    public Transform wavesLeft;
    public Transform wavesRight;

    [Header("Settings")]
    public float splitDistance = 20f;
    public float cascadeDelay = 0.1f;
    public float splitDuration = 1.2f;

    [Header("Wave Settings")]
    public float waveEndY = 20f;
    public float waveDuration = 1.2f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(PlayCinematic());
        }
    }

    IEnumerator PlayCinematic()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 6; i++)
        {
            StartCoroutine(MovePlane(waterLeft.GetChild(i), -splitDistance));
            StartCoroutine(MovePlane(waterRight.GetChild(i), splitDistance));
            StartCoroutine(RiseWave(wavesLeft.GetChild(i), -splitDistance));
            StartCoroutine(RiseWave(wavesRight.GetChild(i), splitDistance));
            yield return new WaitForSeconds(cascadeDelay);
        }
    }

    IEnumerator MovePlane(Transform plane, float targetOffsetX)
    {
        Vector3 startPos = plane.position;
        Vector3 endPos = startPos + new Vector3(targetOffsetX, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < splitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / splitDuration);
            plane.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        plane.position = endPos;
    }

    IEnumerator RiseWave(Transform wave, float targetOffsetX)
    {
        Vector3 startPos = wave.position;
        Vector3 endPos = new Vector3(startPos.x + targetOffsetX, waveEndY, startPos.z);
        float elapsed = 0f;

        while (elapsed < waveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / waveDuration);
            wave.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        wave.position = endPos;
    }
}