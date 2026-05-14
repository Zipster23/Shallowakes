using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TenguPostBattleDialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject bubble;

    [Header("Dialogue Lines")]
    [SerializeField] private string[] lines = new string[]
    {
        "Sharro! You've done it! The Tengu has fallen! [[E]]",
        "Inside the shrine lies the Muramasa blade. Its power has been sealed for centuries... [E]",
        "The blade grants you two abilities. Press 1 to fire a wind slash forward! [E]",
        "And press 2 to dash forward and strike! You will need these to face Susanoo. [E]",
        "Our journey continues. Susanoo's domain awaits. Let us go. [E]"
    };

    [SerializeField] private float textSpeed = 0.05f;

    [Header("Shrine Waypoint")]
    [SerializeField] private GameObject shrineWaypoint;     // your red beam/waypoint object
    [SerializeField] private Collider shrineEntranceTrigger; // trigger collider at shrine entrance

    [Header("References")]
    [SerializeField] private Enemy tenguEnemy;
    [SerializeField] private ScreenFade screenFade;

    private bool dialogueActive = false;
    private bool dialogueStarted = false;
    private bool dialogueFinished = false;
    private int index = 0;




    private void Start()
    {
        // make sure waypoint starts hidden
        if(shrineWaypoint != null) shrineWaypoint.SetActive(false);
        if(shrineEntranceTrigger != null) shrineEntranceTrigger.enabled = false;
    }




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
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }




    // called by shrine entrance trigger when player walks in
    public void OnPlayerReachedShrine()
    {
        if(dialogueFinished)
        {
            StartCoroutine(TransitionToMap());
        }
    }




    private IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        icon.SetActive(true);
        bubble.SetActive(true);
        textComponent.gameObject.SetActive(true);

        dialogueActive = true;
        index = 0;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());
    }




    private IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;

        foreach(char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }




    private void NextLine()
    {
        if(index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            // all lines done
            dialogueActive = false;
            dialogueFinished = true;
            icon.SetActive(false);
            bubble.SetActive(false);
            textComponent.gameObject.SetActive(false);
            textComponent.text = string.Empty;
            StopAllCoroutines();

            // show shrine waypoint and enable entrance trigger
            if(shrineWaypoint != null) shrineWaypoint.SetActive(true);
            if(shrineEntranceTrigger != null) shrineEntranceTrigger.enabled = true;
        }
    }




    private IEnumerator TransitionToMap()
    {
        // hide waypoint
        if(shrineWaypoint != null) shrineWaypoint.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        if(screenFade != null)
            yield return StartCoroutine(screenFade.FadeOut());

        StageProgress.CompleteTenguShrine();
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Main_Menu");
    }
}