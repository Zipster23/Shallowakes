using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SusanooSection1 : MonoBehaviour
{
    
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private SusanooSFXManager sfx;
    [SerializeField] private Transform player;
    [SerializeField] private float timeBetweenSlashes = 5f;
    [SerializeField] private float section1SlashSpeed = 60f;
    [SerializeField] private GameObject windSlashPrefab;
    [SerializeField] private float windSlashSpawnOffset = 2f;

    private bool isActive = false;
    private Coroutine slashCoroutine;




    public void StartSection1()
    {
        
        isActive = true;
        slashCoroutine = StartCoroutine(ThrowSlashesAtPlayer());

    }




    public void EndSection1()
    {
        
        isActive = false;
        if(slashCoroutine != null)
        {
            StopCoroutine(slashCoroutine);
        }

    }




    private IEnumerator ThrowSlashesAtPlayer()
    {
        
        while(isActive)
        {
            yield return new WaitForSeconds(timeBetweenSlashes);

            if(!isActive) yield break;

            // spawn wind slash aimed at player
            sfx.PlayWindSlashSFX();
            Vector3 spawnPos = susanooAI.transform.position + susanooAI.transform.forward * windSlashSpawnOffset;
            Vector3 direction = (player.position - spawnPos).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject slash = Instantiate(windSlashPrefab, spawnPos, rotation);
            WindSlash windSlash = slash.GetComponent<WindSlash>();
            if(windSlash != null)
            {
                windSlash.SetDirection(direction);
                windSlash.speed = section1SlashSpeed;
                windSlash.isParriable = true;
            }
        }

    }

}
