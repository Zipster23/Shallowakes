using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject loreCutscene;
    [SerializeField] private GameObject mapScreen;
    [SerializeField] private GameObject startButton; // Assign the Start button in Inspector

    private void Start()
    {
        // Hide Start button if the player has never completed New Game before
        if (startButton != null)
            startButton.SetActive(PlayerPrefs.HasKey("HasPlayedBefore"));

        if (PlayerPrefs.HasKey("ReturnToMap"))
        {
            PlayerPrefs.DeleteKey("ReturnToMap");
            PlayerPrefs.Save();
            SetActiveUI(mainMenuUI, false);
            SetActiveUI(mapScreen, true);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.R))
        {
            StageProgress.ResetAllProgress();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Hide Start button again after full reset
            if (startButton != null)
                startButton.SetActive(false);

            Debug.Log("All progress reset!");
        }
    }

    public void PlayGame()
    {
        SetActiveUI(mainMenuUI, false);

        if (PlayerPrefs.HasKey("HasPlayedBefore"))
        {
            SetActiveUI(mapScreen, true);
        }
        else
        {
            PlayerPrefs.SetInt("HasPlayedBefore", 1);
            PlayerPrefs.Save();
            SetActiveUI(loreCutscene, true);
        }
    }

    public void NewGame()
    {
        StageProgress.ResetAllProgress();
        PlayerPrefs.DeleteKey("HasPlayedBefore");
        PlayerPrefs.DeleteKey("ReturnToMap");
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();

        SetActiveUI(mainMenuUI, false);
        SetActiveUI(loreCutscene, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetActiveUI(GameObject panel, bool active)
    {
        if (panel == null)
        {
            Debug.LogError("[MainMenuManager] A UI panel reference is null! Check Inspector assignments.");
            return;
        }
        panel.SetActive(active);
    }
}