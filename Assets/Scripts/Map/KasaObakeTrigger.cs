using UnityEngine;

public class KasaObakeTrigger : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject bubble;
    [SerializeField] private GameObject lightBeam;

    private Collider koCollider;
    private bool hasTriggered = false;

    private void Start()
    {
        koCollider = GetComponent<Collider>();
        koCollider.isTrigger = false;

        if (icon != null) icon.SetActive(false);
        if (bubble != null) bubble.SetActive(false);
        if (lightBeam != null) lightBeam.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            hasTriggered = true;
            koCollider.isTrigger = true;

            if (icon != null) icon.SetActive(true);
            if (bubble != null) bubble.SetActive(true);
            if (lightBeam != null) lightBeam.SetActive(false);

            if (tutorialManager != null)
            {
                tutorialManager.OnPlayerMeetKasaObake();
            }
        }
    }
}