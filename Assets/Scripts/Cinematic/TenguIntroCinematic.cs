using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class TenguIntroCinematic : MonoBehaviour
{
   
    [Header("References")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private GameObject player;
    [SerializeField] private TenguAI tenguAI;

    [Header("Music")]
    [SerializeField] private AudioClip tenguTheme;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicSyncTime;               // how many seconds into the cutscene the big bass hit should play
    [SerializeField] private float bassHitTimeInSong = 2.5f;    // how many seconds into the SONG the bass hit occurs

    private bool hasPlayed;          // true once the cinematic has been triggered so it only plays once
    private bool isPlaying;          // true while the cutscene is actively playing
    private float elapsedTime = 0f;  // we track elapsed time manually so we know when to start the music




    private void Update()
    {
        
        if(!isPlaying)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        // When we hit the sync point, start the music from the beginning
        // The bass hit will land at bassHitTimeInSong seconds after this
        if(!musicSource.isPlaying && elapsedTime >= musicSyncTime)
        {
            musicSource.clip = tenguTheme;

            // start the music from the beginning so the full song plays
            // the bass hit naturally arrives at 2.5 seconds in

        }

    }




    // Call this from a trigger collider when the player enters the shrine arena
    public void TriggerCinematic()
    {
        
        if(hasPlayed)
        {
            return;
        }

        hasPlayed = true;

        StartCoroutine(PlayCinematic());

    }




    private IEnumerator PlayCinematic()
    {
        
        isPlaying = true;
        elapsedTime = 0f;

        // lock player input. Disable player's controller and movement so they can't move during the cutscene
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;

        // lock tengu AI. Disable the tengu's AI so it doesn't start attacking on its own while the cutscene is playing
        tenguAI.enabled = false;

        // play the timeline
        timelineDirector.Play();

        // wait for the timeline to finish playingg
        // timeline duration is set by how long you make it in the editor
        yield return new WaitForSeconds((float)timelineDirector.duration);

        // cutscene over. Hand control back
        isPlaying = false;

        // re-enable player controls & Tengu AI
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        tenguAI.enabled = true;

        // Tell the Tengu to immediately do the Dash Slash as its opening move
        tenguAI.TriggerOpeningDashSlash();


    }

}
