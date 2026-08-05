using UnityEngine;
using System.Collections;

public class EnemySpawnIndicator : MonoBehaviour
{
    
    [Header("Settings")]
    public float spawnDelay = 1.5f;

    private GameObject enemyToSpawn;

    public void Initialize(GameObject enemyPrefab)
    {
        enemyToSpawn = enemyPrefab;
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (enemyToSpawn != null)
        {
            ObjectPoolManager.Instance.SpawnObject(enemyToSpawn, transform.position, Quaternion.identity);
        }

        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
