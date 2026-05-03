using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoreCutscene : MonoBehaviour
{
    
    [SerializeField] private Image fadePanel;
    [SerializeField] private Text loreText;
    [SerializeField] private GameObject mapScreen;
    [SerializeField] private float fadeDuration = 1f;

    // all the lore lines that will appear one after another
    private string[] loreLines = new string[]
    {
        "In the age of gods and mortals...",
        "Sharro, a wandering swordsman, lived peacefully with his beloved wife Kamehime.",
        "Until the day Susanoo, the Storm God, descended from the heavens...",
        "And took Kamehime into his domain.",
        "Guided by the light of Amaterasu...",
        "Sharro set forth on a journey through the cursed lands.",
        "His path now leads through the Whispering Forest...",
        "Where the Demon Parade marches under Susanoo's command."
    };




    private void Start()
    {
        
        StartCoroutine(PlayLoreCutscene());

    }




    private IEnumerator PlayLoreCutscene()
    {
        
        // start fully black
        SetAlpha(1f);
        loreText.text = "";

        yield return new WaitForSeconds(0.5f);

        // fade in
        yield return StartCoroutine(Fade(1f, 0f));

        // show each lore line one by one
        foreach(string line in loreLines)
        {
            loreText.text = line;
            yield return new WaitForSeconds(2.5f);

            // fade to black between lines
            yield return StartCoroutine(Fade(0f, 1f));
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(Fade(1f, 0f));
        }

        // fade to black one final time
        yield return StartCoroutine(Fade(0f, 1f));

        // show the map
        gameObject.SetActive(false);
        mapScreen.SetActive(true);

        // fade back in on the map
        yield return StartCoroutine(Fade(1f, 0f));

    }




    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        
        float elapsed = 0f;
        while(elapsed < fadeDuration)
        {
            
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration));
            yield return null;

        }

        SetAlpha(endAlpha);

    }




    private void SetAlpha(float alpha)
    {
        
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;

    }



}
