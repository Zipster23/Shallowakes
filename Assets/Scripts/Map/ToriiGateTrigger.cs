using UnityEngine;
using System.Collections;

public class ToriiGateTrigger : MonoBehaviour
{
    public GameObject[] paradeObjects;
    public float fadeDuration = 2f;
    public string playerTag = "Player";

    private bool _triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag(playerTag)) return;
        _triggered = true;

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
}