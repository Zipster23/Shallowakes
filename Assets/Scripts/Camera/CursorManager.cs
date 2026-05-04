using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    private bool _manuallyUnlocked = false;

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            _manuallyUnlocked = true;
            UnlockCursor();
        }

        if (_manuallyUnlocked && Input.GetMouseButtonDown(0))
        {
            _manuallyUnlocked = false;
            LockCursor();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !_manuallyUnlocked)
            LockCursor();
        else if (!hasFocus)
            UnlockCursor();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _manuallyUnlocked = false;
        if (scene.buildIndex == 0)
            UnlockCursor();
        else
            LockCursor();
    }

    public void LockCursor()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}