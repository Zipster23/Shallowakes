using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class TenguPostBattleDialogue : MonoBehaviour
{
    
    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private GameObject icon;           // kasa obake face image
    [SerializeField] private GameObject bubble;         // white dialogue box panel
    [SerializeField] private GameObject continuePrompt; // "Press E to continue" text

    [Header("Dialogue Lines")]
    [SerializeField] private string[] lines = new string[]
    {
        "Shallo! You've done it! The Tengu has fallen!",
        "Inside the shrine lies the Muramasa blade. Its power has been sealed away for centuries...",
        "The blade will grant you two abilities. Press 1 to unleash a wind slash forward!",
        "And press 2 to dash forward and strike! You will need these to face Susanoo.",
        "Our journey continues. Susanoo's domain awaits. Let us go."
    };

    [SerializeField] private float textSpeed = 0.05f;

    [Header("References")]
    [SerializeField] private Enemy tenguEnemy;
    [SerializeField] private ScreenFade screenFade;

    private bool dialogueActive = false;
    private bool dialogueStarted = false;
    private int index = 0;




    private void Update()
    {
        
        // check if tengu is dead and dialogue hasn't started yet
        if(!dialogueStarted && tenguEnemy != null && tenguEnemy.currentHealth <= 0)
        {
            dialogueStarted = true;
            StartCoroutine(StartDialogueAfterDelay());
        }

        // handle E press to advance dialogue
        if(dialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            if(textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                // skip typing and show full line
                StopAllCoroutines();
                textComponent.text = lines[index];
                StartCoroutine(ShowContinuePrompt());
            }
        }

    }




    private IEnumerator StartDialogueAfterDelay()
    {
        
        // wait for death anim to play
        yield return new WaitForSeconds(4f);

        // show dialogue UI
        icon.SetActive(true);
        bubble.SetActive(true);
        if(continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        dialogueActive = true;
        index = 0;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());

    }




    private IEnumerator TypeLine()
    {
        
        textComponent.text = string.Empty;
        if(continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        foreach(char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        // show continue prompt when line finishes typing
        StartCoroutine(ShowContinuePrompt());

    }




    private IEnumerator ShowContinuePrompt()
    {
        
        yield return new WaitForSeconds(0.3f);
        if(continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }

    }




    private void NextLine()
    {
        
        if(continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        if(index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            // all lines done - hide dialogue and go to map
            StartCoroutine(EndDialogue());
        }

    }




    private IEnumerator EndDialogue()
    {
        
        dialogueActive = false;
        icon.SetActive(false);
        bubble.SetActive(false);
        if(continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }
        textComponent.text = string.Empty;

        yield return new WaitForSeconds(1f);

        // save progress and go to map
        StageProgress.CompleteTenguShrine();
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();

        if(screenFade != null)
        {
            yield return StartCoroutine(screenFade.FadeOut());   
        }

        SceneManager.LoadScene("Main_Menu");

    }

}
