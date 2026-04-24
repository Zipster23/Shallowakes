using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowClone : MonoBehaviour
{
    public GameObject lanternYokaiClone;
    public int cloneCount = 5;
    public float cloneRadius = 6f;
    public Transform player;

    private bool isShadowCloning = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (!isShadowCloning)
        {
            isShadowCloning = true;
            StartCoroutine(ShadowCloneSequence());
        }
    }

    private IEnumerator ShadowCloneSequence()
    {
        for (int i = 0; i < cloneCount; i++)
        {
            float angle = i * (360f / cloneCount);
            float radian = angle * Mathf.Deg2Rad;

            Vector3 spawnPos = new Vector3(
                player.position.x + cloneRadius * Mathf.Cos(radian),
                transform.position.y,
                player.position.z + cloneRadius * Mathf.Sin(radian)
            );

            Instantiate(lanternYokaiClone, spawnPos, Quaternion.identity);
        }

        yield return null;
    }
}