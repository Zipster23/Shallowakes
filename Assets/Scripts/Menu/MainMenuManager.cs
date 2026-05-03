using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject loreCutscene;
    [SerializeField] private GameObject mapScreen;

    public void PlayGame()
    {
        mainMenuUI.SetActive(false);
    
        // if they've played before, skip lore and go straight to map
        if(PlayerPrefs.HasKey("HasPlayedBefore"))
        {
            mapScreen.SetActive(true);
        }
        else
        {
            // first time ever playing
            PlayerPrefs.SetInt("HasPlayedBefore", 1);
            PlayerPrefs.Save();
            loreCutscene.SetActive(true);
        }
    }




    private void Start()
    {
        // if returning from a stage, skip straight to map
        if(PlayerPrefs.HasKey("ReturnToMap"))
        {
            PlayerPrefs.DeleteKey("ReturnToMap");
            mainMenuUI.SetActive(false);
            mapScreen.SetActive(true);
        }
    }




    private void Update()
    {
        // press Ctrl+Shift+R to reset all progress (for testing/new players)
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            StageProgress.ResetAllProgress();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All progress reset!");
        }
    }




    public void NewGame()
    {
        // clear all progress
        StageProgress.ResetAllProgress();
        PlayerPrefs.DeleteKey("HasPlayedBefore");
        PlayerPrefs.DeleteKey("ReturnToMap");
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();

        // start fresh with lore cutscene
        mainMenuUI.SetActive(false);
        loreCutscene.SetActive(true);
    }




    public void QuitGame()
    {
        Application.Quit();
    }
}