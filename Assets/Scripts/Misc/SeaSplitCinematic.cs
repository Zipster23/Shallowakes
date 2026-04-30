using System.Collections;
using UnityEngine;

public class SeaSplitCinematic : MonoBehaviour
{
    
    [SerializeField] private GameObject seaSplitVFX;
    [SerializeField] private GameObject land;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip section1Music;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private SusanooSection1 section1;




    private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player"))
        {
            
            // split sea
            if(seaSplitVFX != null)
            {
                seaSplitVFX.SetActive(true);
                land.SetActive(true);
            } 

            // play section 1 music
            musicSource.clip = section1Music;
            musicSource.Play();

            // start section 1 - susanoo throws slashes
            section1.StartSection1();

            // disable this trigger
            GetComponent<Collider>().enabled = false;

        }

    }

}