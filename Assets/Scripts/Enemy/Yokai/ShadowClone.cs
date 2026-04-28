using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowClone : YokaiAbility
{
    [Header("Shadow Clone Settings")]
    public GameObject lanternYokaiClone;    // the clone prefab to spawn
    public int cloneCount = 5;              // how many clones to spawn
    public float cloneRadius = 6f;          // radius of the circle the clones spawn in
    public int triggerChance = 30;          // % chance to trigger when attack fires
    public float warningDuration = 1f;      // pause after spawning before clones attack
    public float cleanupDelay = 2f;         // how long after thrust before clones are destroyed

    private bool isShadowCloning = false;


    // --- YokaiAbility overrides ---

    public override bool TryTrigger(YokaiAI ai)
    {
        Debug.Log($"ShadowClone TryTrigger called, chance: {triggerChance}, isCloning: {isShadowCloning}");
        // Don't trigger if already running
        if (isShadowCloning) return false;

        // Roll for chance
        if(Random.Range(0, 100) >= triggerChance) return false;

        StartCoroutine(ShadowCloneSequence(ai));
        return true;
    }

    public override bool OnYokaiParried()
    {
        // Default parry stun applies — ability doesn't intercept it
        return false;
    }

    public override void OnInterrupted()
    {
        isShadowCloning = false;
        StopAllCoroutines();
    }


    // --- Ability sequence ---

    private IEnumerator ShadowCloneSequence(YokaiAI ai)
    {
        isShadowCloning = true;

        animator.SetBool("IsMoving", false);

        // Keep a list of spawned clones so we can tell them all to thrust at once
        List<YokaiClone> clones = new List<YokaiClone>();

        // Spawn clones evenly spaced in a circle around the player
        for(int i = 0; i < cloneCount; i++)
        {
            float angle  = i * (360f / cloneCount);
            float radian = angle * Mathf.Deg2Rad;

            Vector3 spawnPos = new Vector3(
                ai.player.position.x + cloneRadius * Mathf.Cos(radian),
                transform.position.y,
                ai.player.position.z + cloneRadius * Mathf.Sin(radian)
            );

            GameObject cloneObj = Instantiate(lanternYokaiClone, spawnPos, Quaternion.identity);

            YokaiClone clone = cloneObj.GetComponent<YokaiClone>();
            if(clone != null)
            {
                clone.Initialize(ai.player, ai.attackDamage, ai.playerLayer);
                clones.Add(clone);
            }
        }

        // After spawning all clones, calculate their center point
        Vector3 centerPoint = Vector3.zero;
        foreach (YokaiClone clone in clones)
            centerPoint += clone.transform.position;
        centerPoint /= clones.Count;

        yield return new WaitForSeconds(warningDuration);

        animator.SetTrigger("Attack");
        ai.FacePlayer();

        // Pass the fixed center point into each clone's thrust
        foreach (YokaiClone clone in clones)
        {
            if (clone != null)
                clone.PerformThrust(centerPoint);
        }

        // Wait for animations to finish then clean up clones
        yield return new WaitForSeconds(cleanupDelay);

        foreach(YokaiClone clone in clones)
        {
            if(clone != null)
            {
                Destroy(clone.gameObject);
            }
        }

        // Done — hand control back to YokaiAI
        isShadowCloning = false;
        ai.NotifyAbilityComplete();
    }
}
