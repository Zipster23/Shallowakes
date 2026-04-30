using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section1CompleteTrigger : MonoBehaviour
{
    
    [SerializeField] private SusanooRespawnManager respawnManager;

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player"))
        {
            respawnManager.hasPassedSection1 = true;
            GetComponent<Collider>().enabled = false;
        }

    }

}
