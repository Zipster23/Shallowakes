using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Row Parents")]
    public GameObject rowA;
    public GameObject rowB;

    [Header("Shared Animation Curves")]
    public AnimationCurve xMotion = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve yMotion = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleLoop = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Transition Curves")]
    public AnimationCurve scaleInEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleOutEase = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Timing & Cascade")]
    public float cycleDuration = 4f;
    public float cascadeDelay = 0.1f;
    public float scaleInDuration = 0.5f;
    public float scaleOutDuration = 0.5f;

    private WaveMotion[] wavesA;
    private WaveMotion[] wavesB;

    void Start()
    {
        wavesA = rowA.GetComponentsInChildren<WaveMotion>();
        wavesB = rowB.GetComponentsInChildren<WaveMotion>();

        // Push settings to all children
        foreach (var w in wavesA) SetupWave(w, false);
        foreach (var w in wavesB) SetupWave(w, true);
    }

    void SetupWave(WaveMotion w, bool mirror)
    {
        w.manager = this;
        w.isMirrored = mirror;
    }

    public void TriggerAllScaleIn()
    {
        StartCoroutine(CascadeIn());
    }

    IEnumerator CascadeIn()
    {
        float startTime = Time.time;
        int count = Mathf.Max(wavesA.Length, wavesB.Length);
        for (int i = 0; i < count; i++)
        {
            float targetTime = startTime + (i * cascadeDelay);
            yield return new WaitUntil(() => Time.time >= targetTime);
            if (i < wavesA.Length) wavesA[i].TriggerScaleIn();
            if (i < wavesB.Length) wavesB[i].TriggerScaleIn();
        }
    }
}