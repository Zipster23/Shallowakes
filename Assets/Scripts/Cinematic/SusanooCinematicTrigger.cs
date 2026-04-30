using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SusanooCinematicTrigger : MonoBehaviour
{
    
    [SerializeField] private SusanooIntroCinematic cinematic;
    [SerializeField] private GameObject arenaBarrier;




    private void OnTriggerEnter(Collider other)
    {
        
        // only trigger if it's the player walking in
        if(other.CompareTag("Player"))
        {
            cinematic.TriggerCinematic();
            arenaBarrier.SetActive(true);
            GetComponent<Collider>().enabled = false;    // disable this trigger so it never fires again
        }

    }

}
