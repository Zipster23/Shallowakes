using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StageProgress
{
    
    // call these when a stage is beaten
    public static void CompleteWhisperingForest()
    {

        PlayerPrefs.SetInt("WhisperingForest_Complete", 1);
        PlayerPrefs.Save();

    }

    public static void CompleteTenguShrine()
    {
        PlayerPrefs.SetInt("TenguShrine_Complete", 1);
        PlayerPrefs.Save();
    }

    public static void CompleteSusanoo()
    {
        PlayerPrefs.SetInt("Susanoo_Complete", 1);
        PlayerPrefs.Save();
    }




    // call these to check progress
    public static bool IsWhisperingForestComplete()
    {
        return PlayerPrefs.GetInt("WhisperingForest_Complete", 0) == 1;
    }

    public static bool IsTenguShrineComplete()
    {
        return PlayerPrefs.GetInt("TenguShrine_Complete", 0) == 1;
    }

    public static bool IsSusanooComplete()
    {
        return PlayerPrefs.GetInt("Susanoo_Complete", 0) == 1;
    }




    // call this to reset all progress
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

}
