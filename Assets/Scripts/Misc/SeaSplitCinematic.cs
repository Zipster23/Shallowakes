using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    
    [SerializeField] private GameObject seaSplitVFX;
    [SerializeField] private GameObject land;
    [HideInInspector]
    [SerializeField] public AudioSource musicSource;
    [SerializeField] private AudioClip section1Music;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private SusanooSFXManager sfx;
    [SerializeField] private SusanooSection1 section1;
    [SerializeField] public GameObject windSlashVerticalPrefab;




    private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player"))
        {
            // fire huge vertical wind slash
            Vector3 spawnPos = susanooAI.transform.position + susanooAI.transform.forward * 2f;
            Vector3 direction = (other.transform.position - spawnPos).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject slash = Instantiate(windSlashVerticalPrefab, spawnPos, rotation);
            WindSlash windSlash = slash.GetComponent<WindSlash>();
            if(windSlash != null)
            {
                windSlash.SetDirection(direction);
                windSlash.speed = 220f;
                windSlash.isParriable = false;
            }

            // split sea
            if(seaSplitVFX != null)
            {
                seaSplitVFX.SetActive(true);
                land.SetActive(true);
            } 

            // play section 1 music
            sfx.PlayWindSlashSFX();
            musicSource.clip = section1Music;
            musicSource.Play();

            // start section 1 - susanoo throws slashes
            section1.StartSection1();

            // disable this trigger
            GetComponent<Collider>().enabled = false;

        }

    }

}