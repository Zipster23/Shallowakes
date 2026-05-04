using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public GameObject[] enemyPrefabs;
}

public class SpawnManager : MonoBehaviour
{
    public Wave[] waves;
    public float groupSpread = 3f;
    public float waveInterval = 5f;

    private GameObject[] activeEnemies;
    private bool waveInProgress = false;
    private int currentWave = 0;

    private void Update()
    {
        if (waveInProgress && AllEnemiesDefeated())
        {
            waveInProgress = false;

            if (currentWave < waves.Length)
                Invoke("SpawnWave", waveInterval);
            else
                OnAllWavesComplete();
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
        if (currentWave >= waves.Length) return;

        GameObject[] prefabs = waves[currentWave].enemyPrefabs;
        activeEnemies = new GameObject[prefabs.Length];

        for (int i = 0; i < prefabs.Length; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-groupSpread, groupSpread),
                0f,
                Random.Range(-groupSpread, groupSpread)
            );
            Vector3 spawnPos = transform.position + offset;
            Quaternion spawnRot = prefabs[i].transform.rotation;
            activeEnemies[i] = Instantiate(prefabs[i], spawnPos, spawnRot);
        }

        currentWave++;
        waveInProgress = true;
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("All waves complete.");
        
    }
}