using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section1CompleteTrigger : MonoBehaviour
{
    
    [SerializeField] private SusanooSection1 section1;
    [SerializeField] private SusanooRespawnManager respawnManager;
    [SerializeField] private SusanooIntroCinematic cinematic;
    [SerializeField] private AudioSource musicSource;




    private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player"))
        {
            
            // stop section 1
            section1.EndSection1();
            musicSource.Stop();

            // mark section 1 as complete for respawn
            respawnManager.hasPassedSection1 = true;

            // trigger the susanoo cinematic
            cinematic.TriggerCinematic();

            // disable trigger
            GetComponent<Collider>().enabled = false;

        }

    }

}
