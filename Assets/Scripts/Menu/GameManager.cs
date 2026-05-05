using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Flag that we're returning mid-game, so the menu shows Continue
            PlayerPrefs.SetInt("ReturnToMenu", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(0);
        }
    }
}