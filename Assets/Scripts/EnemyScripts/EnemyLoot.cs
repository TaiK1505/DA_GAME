using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Prefabs")]
    public GameObject currencyPrefab;
    public GameObject xpPrefab;

    [Header("Currency Drop Rates")]
    [Range(0f, 100f)] public float currencyDropChance = 50f;
    public int minCurrency = 1;
    public int maxCurrency = 3;

    [Header("XP Drop Rates")]
    [Range(0f, 100f)] public float xpDropChance = 100f;
    public int minXp = 1;
    public int maxXp = 2;

    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        // Subscribe to the death event when pulled from the pool
        healthComponent.OnDeath += DropLoot;
    }

    private void OnDisable()
    {
        // Unsubscribe when returned to the pool to prevent memory leaks
        healthComponent.OnDeath -= DropLoot;
    }

    private void DropLoot()
    {
        if (xpPrefab != null && Random.Range(0f, 100f) <= xpDropChance)
        {
            int amount = Random.Range(minXp, maxXp + 1);
            SpawnDrops(xpPrefab, amount);
        }

        if (currencyPrefab != null && Random.Range(0f, 100f) <= currencyDropChance)
        {
            int amount = Random.Range(minCurrency, maxCurrency + 1);
            SpawnDrops(currencyPrefab, amount);
        }
    }

    private void SpawnDrops(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Add a small random scatter so they spread out physically
            Vector2 scatterOffset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPos = transform.position + (Vector3)scatterOffset;

            ObjectPoolManager.Instance.SpawnObject(prefab, spawnPos, Quaternion.identity);
        }
    }
}