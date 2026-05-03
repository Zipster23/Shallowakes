using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float groupSpread = 3f;      // How tightly clustered the yokai spawn
    public float waveInterval = 5f;     // Delay before next wave after all are defeated

    private GameObject[] activeEnemies;
    private bool waveInProgress = false;

    private void Update()
    {
        if (waveInProgress && AllEnemiesDefeated())
        {
            waveInProgress = false;
            Invoke("SpawnWave", waveInterval);
        }
    }

    public void TriggerFirstWave()
    {
        if (!waveInProgress)
            SpawnWave();
    }

    private bool AllEnemiesDefeated()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) return false;
        }
        return true;
    }

    private void SpawnWave()
    {
        activeEnemies = new GameObject[enemyPrefabs.Length];

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-groupSpread, groupSpread),
                0f,
                Random.Range(-groupSpread, groupSpread)
            );

            Vector3 spawnPos = transform.position + offset;
            Quaternion spawnRot = enemyPrefabs[i].transform.rotation;

            activeEnemies[i] = Instantiate(enemyPrefabs[i], spawnPos, spawnRot);
        }

        waveInProgress = true;
    }
}