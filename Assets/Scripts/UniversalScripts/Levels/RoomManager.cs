using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RoomManager : MonoBehaviour
{
    
    [Header("Room Setup")]
    public GameObject[] doors;
    public Transform[] spawnPoints;      

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;    
    public GameObject spawnIndicatorPrefab; 
    public float spawnDelay = 1.5f;      

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool hasTriggered = false;
    private bool roomCleared = false;
    private int enemiesStillSpawning = 0;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            LockDoors();
            SpawnEnemies();
        }
    }

    private void LockDoors()
    {
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(true);
        }
    }

    private void SpawnEnemies()
    {
        enemiesStillSpawning = spawnPoints.Length;
        foreach (Transform point in spawnPoints)
        {
            StartCoroutine(SpawnSequence(point));
        }
    }

    private IEnumerator SpawnSequence(Transform spawnPoint)
    {
        // Pick a random enemy blueprint
        int randomEnemyType = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyToSpawn = enemyPrefabs[randomEnemyType];

        // Spawn the Indicator using the Object Pool
        GameObject indicatorObj = ObjectPoolManager.Instance.SpawnObject(
            spawnIndicatorPrefab, 
            spawnPoint.position, 
            Quaternion.identity
        );

        // Initialize the indicator (so it can show the correct warning visual)
        EnemySpawnIndicator indicatorScript = indicatorObj.GetComponent<EnemySpawnIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.Initialize(enemyToSpawn);
        }

        // Wait for the delay
        yield return new WaitForSeconds(spawnDelay);

        // Return indicator to pool (Turning it off usually returns it in standard pools)
        indicatorObj.SetActive(false); 

        // Spawn the REAL enemy from the Object Pool!
        GameObject newEnemy = ObjectPoolManager.Instance.SpawnObject(
            enemyToSpawn, 
            spawnPoint.position, 
            Quaternion.identity
        );
        
        activeEnemies.Add(newEnemy);
        enemiesStillSpawning--; 
    }

    private void Update()
    {
        if (hasTriggered && !roomCleared)
        {
            CheckEnemies();
        }
    }

    private void CheckEnemies()
    {
        // check if the object pool turned the enemy off!
        activeEnemies.RemoveAll(enemy => !enemy.activeInHierarchy);

        if (activeEnemies.Count == 0 && enemiesStillSpawning == 0)
        {
            UnlockDoors();
        }
    }

    private void UnlockDoors()
    {
        Debug.Log("ROOM CLEARED!");
        roomCleared = true;
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(false);
        }
    }
}
