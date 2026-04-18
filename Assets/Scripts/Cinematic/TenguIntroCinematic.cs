using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class TenguIntroCinematic : MonoBehaviour
{
   
    [Header("Debug")]
    [SerializeField] private bool enableCinematic = true;   // uncheck this in the inspector to skip the cinematic during testing

    [Header("Core References")]
    [SerializeField] private GameObject player;
    [SerializeField] private TenguAI tenguAI;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private ScreenFade fader;

    [Header("Cinematic Cameras")]
    [SerializeField] private CinemachineVirtualCamera camShrine;    // slow pan around the shrine
    [SerializeField] private CinemachineVirtualCamera camTengu;     // medium shot of tengu kneeling
    [SerializeField] private CinemachineVirtualCamera camTenguFace; // close up on the tengu's face

    [Header("Tengu")]
    [SerializeField] private Animator tenguAnimator;

    [Header("Music")]
    [SerializeField] private AudioClip tenguTheme;
    [SerializeField] private AudioClip shrineTheme;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicSyncTime;               // how many seconds into the cutscene the big bass hit should play

    [Header("Timing")]
    [SerializeField] private float shrinePanDuration = 3f;      // how long the camera pans around the shrine
    [SerializeField] private float tenguKneelDuration = 2f;     // how long we look at the Tengu kneeling before moving to his face
    [SerializeField] private float faceHoldDuration = 0.25f;     // how long we look at the Tengu's face before the music plays
    [SerializeField] private float afterMusicDuration = 0.3f;     // how long after the music plays before we fade to black

    private bool hasPlayed;          // true once the cinematic has been triggered so it only plays once
    private bool isPlaying;          // true while the cutscene is actively playing




    public void TriggerCinematic()
    {
        
        // if disabled in the inspector, skip cinematic
        if(!enableCinematic)
        {
            return;
        }

        if(hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        StartCoroutine(PlayCinematic());

    }




    private IEnumerator PlayCinematic()
    {
        
        // disable everything
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;
        tenguAI.enabled = false;

        // force player to be idle
        player.GetComponent<Animator>().SetBool("isMoving", false);
        player.GetComponent<Animator>().SetFloat("SprintScalar", 1f);

        // fade to black
        yield return StartCoroutine(fader.FadeOut());

        yield return new WaitForSeconds(1f);
        // activate the first camera WHILE the screen is black
        freeLookCamera.enabled = false;
        ActivateCamera(camShrine);

        // show title text and play shrine music
        musicSource.volume = 0.5f;
        musicSource.clip = shrineTheme;
        musicSource.Play();
        yield return StartCoroutine(fader.ShowTitle());

        // fade back in
        yield return StartCoroutine(fader.FadeIn());

        // pan around shrine
        yield return new WaitForSeconds(shrinePanDuration);

        // cut to Tengu kneeling
        // tenguAnimator.SetTrigger("Kneel");
        ActivateCamera(camTengu);
        yield return new WaitForSeconds(tenguKneelDuration);

        // cut to Tengu face
        ActivateCamera(camTenguFace);

        // wait 2.5s on the face for the tengu to look up at the camera, then play music
        // tenguAnimator.SetTrigger("LookUp");
        yield return new WaitForSeconds(2.5f);
        musicSource.Stop();
        musicSource.volume = 1f;
        musicSource.clip = tenguTheme;
        musicSource.Play();

        // re-enable everything
        freeLookCamera.enabled = true;
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        tenguAI.enabled = true;

        // deactivate all cinematic cameras so FreeLook Camera takes back over
        DeactivateAllCinematics();

        // straight into gameplay. Tengu immediately dash slashes
        tenguAI.TriggerOpeningDashSlash();

    }




    // Sets one camera to high priority so Cinemachine Brain picks it
    private void ActivateCamera(CinemachineVirtualCamera cam)
    {
        
        // deactivate all cinematic cams first
        camShrine.Priority = 0;
        camTengu.Priority = 0;
        camTenguFace.Priority = 0;

        // activate the one we want
        cam.Priority = 20;

    }




    // Resets all cinematic cameras so FreeLook Camera takes back over
    private void DeactivateAllCinematics()
    {
        
        camShrine.Priority = 0;
        camTengu.Priority = 0;
        camTenguFace.Priority = 0;

    }




    // Call this before ActivateCamera() to control how fast that specific transition is
    // duration = 0 (instant cut), duration = 0.5 (default smooth), duration = 2 (slow cinematic drift)
    private void SetBlendDuration(float duration)
    {
        
        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = duration;

    }   

}
