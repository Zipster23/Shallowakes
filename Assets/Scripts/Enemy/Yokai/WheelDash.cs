using System.Collections;
using UnityEngine;

public class WheelDash : YokaiAbility
{
    [Header("Wheel Dash Settings")]
    public float triggerRange = 15f;
    public float dashSpeed = 60f;
    public float dashDistance = 10f;  // how far it travels before stopping
    public int triggerChance = 100;
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public LayerMask groundLayer;

    private bool isDashing = false;

    private void Update()
    {
        StickToGround();
    }

    public override bool TryTrigger(YokaiAI ai)
    {
        if (Random.Range(0, 100) >= triggerChance) return false;
        StartCoroutine(WheelDashSequence(ai));
        return true;
    }

    public override bool OnYokaiParried()
    {
        isDashing = false;
        StopAllCoroutines();
        return false;
    }

    public override void OnInterrupted()
    {
        isDashing = false;
        StopAllCoroutines();
    }

    private IEnumerator WheelDashSequence(YokaiAI ai)
    {
        isDashing = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // disable physics during dash
        }

        ai.FacePlayer();
        Vector3 dashStart = transform.position;
        Vector3 dashTarget = transform.position + transform.forward * dashDistance;

        yield return new WaitForSeconds(0.3f);

        float elapsed = 0f;
        float duration = dashDistance / dashSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = dashCurve.Evaluate(elapsed / duration);
            transform.position = Vector3.Lerp(dashStart, dashTarget, t);
            StickToGround();
            yield return null;
        }

        transform.position = dashTarget;
        yield return new WaitForSeconds(0.5f);

        if (rb != null)
        {
            rb.isKinematic = false; // re-enable physics after dash
        }

        isDashing = false;
        ai.OnAttackEnd();
    }
    private void StickToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }
}