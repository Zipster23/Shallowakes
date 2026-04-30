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
        if (!isDashing)
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
        Collider col = GetComponent<Collider>();

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.isTrigger = true; // Pass through player during dash

        ai.FacePlayer();

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        yield return new WaitForSeconds(0.3f);

        float elapsed = 0f;
        float duration = dashDistance / dashSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 horizontalPos = transform.position + flatForward * (dashSpeed * Time.deltaTime);

            Vector3 rayOrigin = new Vector3(horizontalPos.x, transform.position.y + 3f, horizontalPos.z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                horizontalPos.y = hit.point.y + 0.05f;
            }

            rb.MovePosition(horizontalPos);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(0.5f);

        if (col != null) col.isTrigger = false; // Restore collider
        if (rb != null) rb.isKinematic = false;

        isDashing = false;
        ai.OnAttackEnd();
    }

    private void StickToGround()
    {
        float radius = 0.3f; // Match roughly to your enemy's collider radius

        if (Physics.SphereCast(transform.position + Vector3.up * 2f, radius, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 0.05f, transform.position.z);
        }
    }
}