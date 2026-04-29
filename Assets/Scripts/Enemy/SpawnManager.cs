using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    public float spawnRangeX = 20;
    public float spawnPosY = 10;
    public float spawnPosZ = 20;

    public float startDelay = 2;
    public float recheckInterval = 0.5f; // How often to check if an enemy needs respawning

    // Tracks the live instance of each enemy type
    private GameObject[] activeEnemies;

    private void Start()
    {
        activeEnemies = new GameObject[enemyPrefabs.Length];
        InvokeRepeating("CheckAndSpawnEnemies", startDelay, recheckInterval);
    }

    private void CheckAndSpawnEnemies()
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            // If the slot is empty (never spawned, or was destroyed), spawn a new one
            if (activeEnemies[i] == null)
            {
                activeEnemies[i] = SpawnEnemy(i);
            }
        }
    }

    private GameObject SpawnEnemy(int index)
    {
        Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnPosY, spawnPosZ);
        return Instantiate(enemyPrefabs[index], spawnPos, enemyPrefabs[index].transform.rotation);
    }
}