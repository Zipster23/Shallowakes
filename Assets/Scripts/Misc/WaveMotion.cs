using UnityEngine;

public class WaveMotion : MonoBehaviour
{
    public enum ScaleState { Idle, ScalingIn, Looping, ScalingOut }
    public ScaleState state = ScaleState.Idle;

    [HideInInspector] public WaveManager manager;
    [HideInInspector] public bool isMirrored = false;

    private Vector3 startPos;
    private float stateStartTime;
    private float waveStartTime;
    private float originalYScale; // Now we store the height (Y)

    void Start()
    {
        startPos = transform.localPosition;
        originalYScale = transform.localScale.y; // Store original height

        // Start with X and Z at 0, but keep Y at original height
        transform.localScale = new Vector3(0, originalYScale, 0);
    }

    public void TriggerScaleIn()
    {
        state = ScaleState.ScalingIn;
        stateStartTime = Time.time;
        waveStartTime = Time.time;
    }

    void Update()
    {
        if (state == ScaleState.Idle) return;

        float elapsed = Time.time - stateStartTime;
        float waveT = ((Time.time - waveStartTime) % manager.cycleDuration) / manager.cycleDuration;

        // 1. HANDLE SCALING
        if (state == ScaleState.ScalingIn)
        {
            float t = elapsed / manager.scaleInDuration;
            float curveStep = manager.scaleInEase.Evaluate(t);

            // Scale X and Z based on curve, keep Y constant
            transform.localScale = new Vector3(curveStep, originalYScale, curveStep);

            if (t >= 1f) state = ScaleState.Looping;
        }
        else if (state == ScaleState.ScalingOut)
        {
            float t = elapsed / manager.scaleOutDuration;
            float curveStep = manager.scaleOutEase.Evaluate(t);

            transform.localScale = new Vector3(curveStep, originalYScale, curveStep);

            if (t >= 1f)
            {
                transform.localScale = new Vector3(0, originalYScale, 0);
                state = ScaleState.Idle;
            }
        }

        // 2. HANDLE MOTION
        // (X/Y position curves from the Manager still apply here)
        float x = manager.xMotion.Evaluate(waveT) * (isMirrored ? -1 : 1);
        float y = manager.yMotion.Evaluate(waveT);

        transform.localPosition = startPos + new Vector3(x, y, 0);
    }
}