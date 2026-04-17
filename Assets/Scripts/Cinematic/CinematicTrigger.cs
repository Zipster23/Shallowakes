using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    
    [SerializeField] private TenguIntroCinematic cinematic;




    private void OnTriggerEnter(Collider other)
    {
        
        // only trigger if it's the player walking in
        if(other.CompareTag("Player"))
        {
            cinematic.TriggerCinematic();
        }

    }

}
