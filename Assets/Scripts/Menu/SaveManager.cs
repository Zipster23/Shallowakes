using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static void SaveProgress(string sceneName)
    {
        PlayerPrefs.SetString("LastScene", sceneName);
        PlayerPrefs.Save();
    }

    public static string LoadProgress()
    {
        return PlayerPrefs.GetString("LastScene", "Tengu_Map");
    }

    public static void DeleteProgress()
    {
        PlayerPrefs.DeleteKey("LastScene");
    }
}