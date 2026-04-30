using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class SusanooIntroCinematic : MonoBehaviour
{
   
    [Header("Debug")]
    [SerializeField] private bool enableCinematic = true;   // uncheck this in the inspector to skip the cinematic during testing

    [Header("Core References")]
    [SerializeField] private GameObject player;
    [SerializeField] private SusanooVFXManager susanooVFX;
    [SerializeField] private SusanooSFXManager susanooSFX;
    [SerializeField] private SusanooAI susanooAI;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private ScreenFade fader;

    [Header("Cinematic Cameras")]
    private int currentPriority = 20;
    [SerializeField] private List<CinemachineVirtualCamera> shrineCameras;

    [Header("Tengu")]
    [SerializeField] private Animator susanooAnimator;

    [Header("Music")]
    [SerializeField] private AudioClip susanooTheme;
    [HideInInspector]
    [SerializeField] public AudioSource musicSource;

    private bool hasPlayed;          // true once the cinematic has been triggered so it only plays once
    private bool isPlaying;          // true while the cutscene is actively playing




    public void TriggerCinematic()
    {
        
        // if disabled in the inspector, skip cinematic
        if(!enableCinematic)
        {
            susanooAI.gameObject.SetActive(true);
            return;
        }

        if(hasPlayed)
        {
            // cinematic already watched. Skip it, but still play fight music
            susanooAI.gameObject.SetActive(true);
            musicSource.clip = susanooTheme;
            musicSource.volume = 1f;
            musicSource.Play();
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
        susanooAI.enabled = false;

        // force player to be idle
        player.GetComponent<Animator>().SetBool("isMoving", false);
        player.GetComponent<Animator>().SetFloat("SprintScalar", 1f);

        // fade to black
        yield return StartCoroutine(fader.FadeOut());

        yield return new WaitForSeconds(1f);

        // activate the first camera WHILE the screen is black
        freeLookCamera.enabled = false;
        ActivateCamera(shrineCameras[0]);

        player.transform.position = new Vector3(618.0583f, 10.39f, 817.12f);

        // show title text and play shrine music
        musicSource.volume = 0.5f;
        musicSource.clip = susanooTheme;
        musicSource.Play();
        yield return StartCoroutine(fader.ShowTitle());

        // fade back in
        yield return StartCoroutine(fader.FadeIn());

        // SHINE TOUR
        SetBlendDuration(0f);
        for(int i = 1; i < shrineCameras.Count; i++)
        {
            ActivateCamera(shrineCameras[i]);
            yield return new WaitForSeconds(3f);
        }

        // re-enable everything
        SetBlendDuration(0.2f);
        freeLookCamera.enabled = true;
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        susanooAI.enabled = true;

        // deactivate all cinematic cameras so FreeLook Camera takes back over
        DeactivateAllCinematics();

    }




    // Sets one camera to high priority so Cinemachine Brain picks it
    private void ActivateCamera(CinemachineVirtualCamera cam)
    {
        
        currentPriority++;
        cam.Priority = currentPriority;

    }




    // Resets all cinematic cameras so FreeLook Camera takes back over
    private void DeactivateAllCinematics()
    {
        
        foreach(CinemachineVirtualCamera cam in shrineCameras)
        {
            cam.Priority = 0;
        }

        currentPriority = 20;

    }




    // Call this before ActivateCamera() to control how fast that specific transition is
    // duration = 0 (instant cut), duration = 0.5 (default smooth), duration = 2 (slow cinematic drift)
    private void SetBlendDuration(float duration)
    {
        
        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = duration;

    }   

}
