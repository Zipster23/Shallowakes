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
        if (isShadowCloning) return false;

        // Only trigger from attack range, not during chase
        float distance = Vector3.Distance(transform.position, ai.player.position);
        if (distance > ai.attackRange) return false;

        if (Random.Range(0, 100) >= triggerChance) return false;

        StartCoroutine(ShadowCloneSequence(ai));
        return true;
    }

    private List<YokaiClone> activeClones = new List<YokaiClone>();

    public override void OnInterrupted()
    {
        isShadowCloning = false;
        StopAllCoroutines();
        CleanUpClones();
    }

    public override bool OnYokaiParried()
    {
        return false;
    }

    private void CleanUpClones()
    {
        foreach (YokaiClone clone in activeClones)
        {
            if (clone != null)
                clone.CleanUp();
        }
        activeClones.Clear();
    }


    // --- Ability sequence ---

    private IEnumerator ShadowCloneSequence(YokaiAI ai)
    {
        isShadowCloning = true;
        activeClones.Clear();
        animator.SetBool("IsMoving", false);

        for (int i = 0; i < cloneCount; i++)
        {
            float angle = i * (360f / cloneCount);
            float radian = angle * Mathf.Deg2Rad;
            Vector3 spawnPos = new Vector3(
                ai.player.position.x + cloneRadius * Mathf.Cos(radian),
                transform.position.y,
                ai.player.position.z + cloneRadius * Mathf.Sin(radian)
            );
            GameObject cloneObj = Instantiate(lanternYokaiClone, spawnPos, Quaternion.identity);
            YokaiClone clone = cloneObj.GetComponent<YokaiClone>();
            if (clone != null)
            {
                clone.Initialize(ai.player, ai.attackDamage, ai.playerLayer);
                activeClones.Add(clone); // track it
            }
        }

        Vector3 centerPoint = Vector3.zero;
        foreach (YokaiClone clone in activeClones)
            centerPoint += clone.transform.position;
        centerPoint /= activeClones.Count;

        yield return new WaitForSeconds(warningDuration);
        animator.SetTrigger("Attack");
        ai.FacePlayer();

        foreach (YokaiClone clone in activeClones)
        {
            if (clone != null)
                clone.PerformThrust(centerPoint);
        }

        yield return new WaitForSeconds(cleanupDelay);
        CleanUpClones();

        isShadowCloning = false;
        ai.NotifyAbilityComplete();
    }
}
