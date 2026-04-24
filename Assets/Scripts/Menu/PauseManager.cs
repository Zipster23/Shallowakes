using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public CinemachineFreeLook freeLookCam;
    public bool paused = false;

    private CinemachineBrain brain;

    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public void TogglePause()
    {
        paused = !paused;
        pauseMenuUI.SetActive(paused);
        freeLookCam.m_XAxis.m_InputAxisName = paused ? "" : "Mouse X";
        freeLookCam.m_YAxis.m_InputAxisName = paused ? "" : "Mouse Y";
        freeLookCam.enabled = !paused;
        brain.enabled = !paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        paused = false;
        pauseMenuUI.SetActive(false);
        freeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
        freeLookCam.m_YAxis.m_InputAxisName = "Mouse Y";
        freeLookCam.enabled = true;
        brain.enabled = true;
        Time.timeScale = 1f;
    }

    public void QuitToMenu()
    {
        freeLookCam.enabled = true;
        brain.enabled = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu");
    }
}