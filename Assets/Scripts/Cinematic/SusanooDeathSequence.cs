using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SusanooDeathSequence : MonoBehaviour
{
    
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private ScreenFade fader;




    public void StartDeathEnding()
    {
        StartCoroutine(SusanooDeathEnding());
    }




    private IEnumerator SusanooDeathEnding()
    {
        musicSource.Stop();
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(fader.FadeOut());

        fader.SetTitleText("Susanoo has fallen.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("The storms that plagued the land began to fade.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("Shallo had fulfilled his oath.");
        yield return StartCoroutine(fader.ShowTitle());

        fader.SetTitleText("Kamehime was free.");
        yield return StartCoroutine(fader.ShowTitle());

        // mark complete and return to map
        StageProgress.CompleteSusanoo();
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Main_Menu");
    }

}
