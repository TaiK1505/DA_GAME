using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Types")]
    public GameObject[] enemyPrefabs; // Array so you can drop in Melee AND Ranged prefabs!

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;   // The corners of your room

    [Header("Wave Settings")]
    public float spawnInterval = 3f;  // Seconds between spawns

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
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0) return;

        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyToSpawn = enemyPrefabs[randomEnemy]; // The prefab blueprint

        int randomPoint = Random.Range(0, spawnPoints.Length);
        Transform chosenPoint = spawnPoints[randomPoint];

        
        ObjectPoolManager.Instance.SpawnObject(enemyToSpawn, chosenPoint.position, Quaternion.identity);
    }
}
