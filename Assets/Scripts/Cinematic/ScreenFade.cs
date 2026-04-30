using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    
    [SerializeField] private Image fadePanel;               // black panel that covers the screen
    [SerializeField] private GameObject titleTextObject;    // text that shows stage title
    [SerializeField] private float fadeDuration = 1f;       // how long the fade in/out takes in seconds
    [SerializeField] private float titleHoldDuration = 5f;  // how long the title text stays visible before fading out




    private void Awake()
    {
        
        // start fully transparent and title hidden
        SetAlpha(0f);
        if(titleTextObject != null)
        {
            titleTextObject.SetActive(false);
        }

    }



    // Fades from black to clear
    public IEnumerator FadeIn()
    {
        
        float elapsed = 0f;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - (elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);

    }




    // Fades from clear to black
    public IEnumerator FadeOut()
    {
        
        float elapsed = 0f;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(elapsed / fadeDuration);
            yield return null;
        }

        SetAlpha(1f);

    }




    // Shows the title text, waits, then hides it
    public IEnumerator ShowTitle()
    {
        
        if(titleTextObject != null)
        {
            titleTextObject.SetActive(true);
            yield return new WaitForSeconds(titleHoldDuration);
            titleTextObject.SetActive(false);
        }

    }




    private void SetAlpha(float alpha)
    {
        
        if(fadePanel == null)
        {
            return;
        }

        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;

    }




    public void SetTitleText(string text)
    {
        if (titleTextObject != null)
            titleTextObject.GetComponent<UnityEngine.UI.Text>().text = text;
    }


}
