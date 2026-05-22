using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SusanooCinematicTrigger : MonoBehaviour
{
    
    [SerializeField] private SusanooIntroCinematic cinematic;
    [SerializeField] private GameObject arenaBarrier;
    [SerializeField] private SeaSplitCinematic seaSplitCinematic;
    [SerializeField] private GameObject bigSusanoo;
    [SerializeField] private GameObject smallSusanoo;




    private void OnTriggerEnter(Collider other)
    {
        
        // only trigger if it's the player walking in
        if(other.CompareTag("Player"))
        {
            bigSusanoo.SetActive(false);
            smallSusanoo.SetActive(true);
            seaSplitCinematic.musicSource.Stop();
            cinematic.TriggerCinematic();
            arenaBarrier.SetActive(true);
            GetComponent<Collider>().enabled = false;    // disable this trigger so it never fires again
        }

    }

}
