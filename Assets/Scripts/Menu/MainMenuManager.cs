using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SaveManager.LoadProgress());
    }

    public void NewGame()
    {
        SaveManager.DeleteProgress();
        SceneManager.LoadScene("Tengu_Map");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}