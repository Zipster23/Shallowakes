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
        string lastScene = SaveManager.LoadProgress();

        // if there's a saved scene, skip lore and go straight to map
        if(PlayerPrefs.HasKey("LastScene"))
        {
            mainMenuUI.SetActive(false);
            mapScreen.SetActive(true);
        }
        else
        {
            // first time playing, show lore cutscene
            mainMenuUI.SetActive(false);
            loreCutscene.SetActive(true);
        }
    }

    public void NewGame()
    {
        SaveManager.DeleteProgress();
        StageProgress.ResetAllProgress();
        mainMenuUI.SetActive(false);
        loreCutscene.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}