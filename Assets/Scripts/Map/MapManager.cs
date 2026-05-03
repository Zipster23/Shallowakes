using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    
    [Header("Stage Sketches")]
    [SerializeField] private GameObject whisperingForestSketch;
    [SerializeField] private GameObject tenguShrineSketch;
    [SerializeField] private GameObject susanooSketch;

    [Header("Arrows")]
    [SerializeField] private GameObject arrowForestToTengu;
    [SerializeField] private GameObject arrowTenguToSusanoo;

    [Header("Stage 1 Objectives")]
    [SerializeField] private GameObject stage1Objectives;
    [SerializeField] private GameObject stage1Objective1Box;
    [SerializeField] private GameObject stage1Objective2Box;
    [SerializeField] private GameObject stage1Objective1Checkmark;
    [SerializeField] private GameObject stage1Objective2Checkmark;

    [Header("Stage 2 Objectives")]
    [SerializeField] private GameObject stage2Objectives;
    [SerializeField] private GameObject stage2Objective1Box;
    [SerializeField] private GameObject stage2Objective2Box;
    [SerializeField] private GameObject stage2Objective1Checkmark;
    [SerializeField] private GameObject stage2Objective2Checkmark;

    [Header("Stage 3 Objectives")]
    [SerializeField] private GameObject stage3Objectives;
    [SerializeField] private GameObject stage3Objective1Box;
    [SerializeField] private GameObject stage3Objective2Box;
    [SerializeField] private GameObject stage3Objective1Checkmark;
    [SerializeField] private GameObject stage3Objective2Checkmark;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkmarkSFX;        // brushstroke sound
    [SerializeField] private AudioClip newObjectivesSFX;    // sound when new objectives appear
    [SerializeField] private AudioClip sketchAppearSFX;     // sound when new sketch appears

    [Header("Fade Settings")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float checkmarkDelay = 0.5f;   // delay between each checkmark appearing
    [SerializeField] private float objectiveFadeDuration = 0.8f;

    [Header("Scene Names")]
    [SerializeField] private string whisperingForestScene = "Whispering_Forest";
    [SerializeField] private string tenguShrineScene = "Tengu_Map";
    [SerializeField] private string susanooScene = "Susanoo_Domain";




    private void OnEnable()
    {
        
        StartCoroutine(ShowMap());

    }




    private IEnumerator ShowMap()
    {
        
        bool forestDone = StageProgress.IsWhisperingForestComplete();
        bool tenguDone = StageProgress.IsTenguShrineComplete();
        bool susanooDone = StageProgress.IsSusanooComplete();

        // hide everything first
        HideAll();

        // always show whispering forest sketch
        yield return StartCoroutine(FadeInGameObject(whisperingForestSketch));

        if(!forestDone)
        {
            // stage 1 not done - show stage 1 objectives
            yield return StartCoroutine(FadeInGameObject(stage1Objectives));
        }
        else if(!tenguDone)
        {
            // forest done, tengu not done
            // show completed stage 1 with checkmarks then transitions to stage 2
            yield return StartCoroutine(ShowCompletedStage1());

            // wait a moment
            yield return new WaitForSeconds(1f);

            // fade out stage 1 objectives
            yield return StartCoroutine(FadeOutGameObject(stage1Objectives));

            // show arrow and tengu sketch
            yield return new WaitForSeconds(0.5f);
            PlaySFX(sketchAppearSFX);
            yield return StartCoroutine(FadeInGameObject(arrowForestToTengu));
            yield return StartCoroutine(FadeInGameObject(tenguShrineSketch));

            // show stage 2 objectives
            yield return new WaitForSeconds(0.3f);
            PlaySFX(newObjectivesSFX);
            yield return StartCoroutine(FadeInGameObject(stage2Objectives));
        }
        else if(!susanooDone)
        {
            // tengu done, susanoo not done
            // show everything from stage 1 and 2 already complete
            whisperingForestSketch.SetActive(true);
            arrowForestToTengu.SetActive(true);
            tenguShrineSketch.SetActive(true);

            // show completed stage 2 with checkmarks
            yield return StartCoroutine(ShowCompletedStage2());

            // wait a moment
            yield return new WaitForSeconds(1f);

            // fade out stage 2 objectives
            yield return StartCoroutine(FadeOutGameObject(stage2Objectives));

            // show arrow and susanoo sketch
            yield return new WaitForSeconds(0.5f);
            PlaySFX(sketchAppearSFX);
            yield return StartCoroutine(FadeInGameObject(arrowTenguToSusanoo));
            yield return StartCoroutine(FadeInGameObject(susanooSketch));

            // show stage 3 objectives
            yield return new WaitForSeconds(0.3f);
            PlaySFX(newObjectivesSFX);
            yield return StartCoroutine(FadeInGameObject(stage3Objectives));
        }
        else
        {
            // everything done - show full map with all completed
            whisperingForestSketch.SetActive(true);
            arrowForestToTengu.SetActive(true);
            tenguShrineSketch.SetActive(true);
            arrowTenguToSusanoo.SetActive(true);
            susanooSketch.SetActive(true);
        }

    }




    private IEnumerator ShowCompletedStage1()
    {
        
        // show stage 1 objectives with boxes
        stage1Objectives.SetActive(true);
        stage1Objective1Box.SetActive(true);
        stage1Objective2Box.SetActive(true);

        // hide checkmarks initially
        stage1Objective1Checkmark.SetActive(false);
        stage1Objective2Checkmark.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // show checkmarks one by one with sound
        PlaySFX(checkmarkSFX);
        stage1Objective1Checkmark.SetActive(true);
        yield return new WaitForSeconds(checkmarkDelay);

        PlaySFX(checkmarkSFX);
        stage1Objective2Checkmark.SetActive(true);
        yield return new WaitForSeconds(checkmarkDelay);

    }




    private IEnumerator ShowCompletedStage2()
    {
        
        // show stage 2 objectives with boxes
        stage2Objectives.SetActive(true);
        stage2Objective1Box.SetActive(true);
        stage2Objective2Box.SetActive(true);

        // hide checkmarks initially
        stage2Objective1Checkmark.SetActive(false);
        stage2Objective2Checkmark.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // show checkmarks one by one with sound
        PlaySFX(checkmarkSFX);
        stage2Objective1Checkmark.SetActive(true);
        yield return new WaitForSeconds(checkmarkDelay);

        PlaySFX(checkmarkSFX);
        stage2Objective2Checkmark.SetActive(true);
        yield return new WaitForSeconds(checkmarkDelay);

    }




    private void HideAll()
    {
        
        // hide all sketches
        whisperingForestSketch.SetActive(false);
        tenguShrineSketch.SetActive(false);
        susanooSketch.SetActive(false);

        // hide all arrows
        arrowForestToTengu.SetActive(false);
        arrowTenguToSusanoo.SetActive(false);

        // hide all objectives
        stage1Objectives.SetActive(false);
        stage2Objectives.SetActive(false);
        stage3Objectives.SetActive(false);

        // hide all checkmarks
        stage1Objective1Checkmark.SetActive(false);
        stage1Objective2Checkmark.SetActive(false);
        stage2Objective1Checkmark.SetActive(false);
        stage2Objective2Checkmark.SetActive(false);
        stage3Objective1Checkmark.SetActive(false);
        stage3Objective2Checkmark.SetActive(false);

    }




    // fades a GameObject in by lerping its CanvasGroup alpha
    private IEnumerator FadeInGameObject(GameObject obj)
    {

        obj.SetActive(true);

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if(cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        float elapsed = 0f;

        while(elapsed < objectiveFadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / objectiveFadeDuration);
            yield return null;
        }

        cg.alpha = 1f;

    }




    // fades a GameObject out by lerping its CanvasGroup alpha
    private IEnumerator FadeOutGameObject(GameObject obj)
    {

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if(cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        cg.alpha = 1f;
        float elapsed = 0f;

        while(elapsed < objectiveFadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / objectiveFadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        obj.SetActive(false);

    }




    private void PlaySFX(AudioClip clip)
    {

        if(audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }

    }




    // called by button on WhisperingForest sketch
    public void OnWhisperingForestClicked()
    {

        StartCoroutine(LoadScene(whisperingForestScene));
    }


    // called by button on TenguShrine sketch
    public void OnTenguShrineClicked()
    {

        if(StageProgress.IsWhisperingForestComplete())
        {
            StartCoroutine(LoadScene(tenguShrineScene));
        }

    }

    // called by button on Susanoo sketch
    public void OnSusanooClicked()
    {

        if(StageProgress.IsTenguShrineComplete())
        {
            StartCoroutine(LoadScene(susanooScene));
        }

    }




    private IEnumerator LoadScene(string sceneName)
    {

        // save which scene we're going to
        SaveManager.SaveProgress(sceneName);

        float elapsed = 0f;
        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = fadePanel.color;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);

    }






}
