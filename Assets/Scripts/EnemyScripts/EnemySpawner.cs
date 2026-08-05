using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Types")]
    public GameObject[] enemyPrefabs;    
    public GameObject spawnIndicatorPrefab;

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;  
    [Header("Wave Settings")]
    public float spawnInterval = 3f;  

    private float nextSpawnTime;

    

    private void Start()
    {
        nextSpawnTime = Time.time + 1f; 
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnRandomEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0 || spawnIndicatorPrefab == null) return;

        // 1. Pick a random enemy blueprint
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyToSpawn = enemyPrefabs[randomEnemy];

        // 2. Pick a random corner from your array
        int randomPoint = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomPoint];

        // 3. Spawn the Indicator at that exact corner instead of the Enemy
        GameObject indicatorObj = ObjectPoolManager.Instance.SpawnObject(
            spawnIndicatorPrefab, 
            chosenSpawnPoint.position, 
            Quaternion.identity
        );
        
        // 4. Hand the indicator the enemy blueprint so it knows what to summon
        EnemySpawnIndicator indicatorScript = indicatorObj.GetComponent<EnemySpawnIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.Initialize(enemyToSpawn);
        }
    
    }    
}
