using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class EnemySpawnWeight
{
    public GameObject enemyPrefab;
    [Tooltip("Higher number = more likely to spawn compared to others in this list")]
    public float spawnWeight = 1f; 
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave 1"; 
    public int minEnemies = 3;
    public int maxEnemies = 5;
    public List<EnemySpawnWeight> enemyPool; 
}

public class RoomManager : MonoBehaviour
{
    [Header("Room Setup")]
    public GameObject[] doors;
    public Transform[] spawnPoints;      

    [Header("Wave Spawning")]
    public List<Wave> waves;                 
    public GameObject spawnIndicatorPrefab; 
    public float spawnDelay = 1.5f;      

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool hasTriggered = false;
    private bool roomCleared = false;
    private int enemiesStillSpawning = 0;
    
    private int currentWaveIndex = 0; 
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            LockDoors();
            StartNextWave(); 
        }
    }

    private void LockDoors()
    {
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(true);
        }
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            UnlockDoors();
            return;
        }

        Wave currentWave = waves[currentWaveIndex];

        int spawnCount = Random.Range(currentWave.minEnemies, currentWave.maxEnemies + 1);
        spawnCount = Mathf.Min(spawnCount, spawnPoints.Length);
        
        enemiesStillSpawning = spawnCount;

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < spawnCount; i++)
        {
            int randSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform selectedPoint = availableSpawns[randSpawnIndex];
            availableSpawns.RemoveAt(randSpawnIndex);

            GameObject enemyToSpawn = GetRandomEnemyFromWave(currentWave);

            StartCoroutine(SpawnSequence(selectedPoint, enemyToSpawn));
        }
    }

    private GameObject GetRandomEnemyFromWave(Wave wave)
    {
        float totalWeight = 0f;
        foreach (EnemySpawnWeight ew in wave.enemyPool) 
        {
            totalWeight += ew.spawnWeight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        
        foreach (EnemySpawnWeight ew in wave.enemyPool)
        {
            if (randomVal < ew.spawnWeight)
            {
                return ew.enemyPrefab;
            }
            randomVal -= ew.spawnWeight;
        }
        
        return wave.enemyPool[0].enemyPrefab;
    }

    private IEnumerator SpawnSequence(Transform spawnPoint, GameObject enemyToSpawn)
    {
        GameObject indicatorObj = ObjectPoolManager.Instance.SpawnObject(
            spawnIndicatorPrefab, 
            spawnPoint.position, 
            Quaternion.identity
        );

        EnemySpawnIndicator indicatorScript = indicatorObj.GetComponent<EnemySpawnIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.Initialize(enemyToSpawn);
        }

        // THIS IS THE WAITING PART!
        yield return new WaitForSeconds(spawnDelay);

        indicatorObj.SetActive(false); 

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
        activeEnemies.RemoveAll(enemy => !enemy.activeInHierarchy);

        if (activeEnemies.Count == 0 && enemiesStillSpawning == 0)
        {
            currentWaveIndex++;
            StartNextWave();
        }
    }

    private void UnlockDoors()
    {
        Debug.Log("ALL WAVES CLEARED!");
        roomCleared = true;
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(false);
        }

        if (GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.TriggerRoomClearSlowMo();
        }

        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(false);
        }
    }
}