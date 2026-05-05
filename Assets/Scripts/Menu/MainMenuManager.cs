using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject loreCutscene;
    [SerializeField] private GameObject mapScreen;
    [SerializeField] private GameObject startButton; // The "Continue" button

    private void Start()
    {
        // Show Continue button if the player has played before OR escaped back mid-game
        bool hasPlayed = PlayerPrefs.HasKey("HasPlayedBefore");
        bool returnedMidGame = PlayerPrefs.HasKey("ReturnToMenu");

        if (startButton != null)
            startButton.SetActive(hasPlayed || returnedMidGame);

        // If they escaped back mid-game, stay on the main menu (don't jump to map)
        if (returnedMidGame)
        {
            PlayerPrefs.DeleteKey("ReturnToMenu");
            PlayerPrefs.Save();
            SetActiveUI(mainMenuUI, true);
            SetActiveUI(mapScreen, false);
            return;
        }

        // If they quit from the map screen previously, jump straight back to map
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
        PlayerPrefs.DeleteKey("ReturnToMenu");
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