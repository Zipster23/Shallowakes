using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineEntranceTrigger : MonoBehaviour
{
    
    [SerializeField] private TenguPostBattleDialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            dialogue.OnPlayerReachedShrine();
            GetComponent<Collider>().enabled = false;
        }
    }

}
