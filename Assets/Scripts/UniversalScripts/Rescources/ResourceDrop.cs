using UnityEngine;

public class ResourceDrop : Pickup
{
    public enum ResourceType { XP, Currency }
    
    [Header("Resource Settings")]
    public ResourceType type;
    public int resourceValue = 10;

    protected override void OnCollected(GameObject player)
    {
        // 1. Give the stats to the player based on the type
        if (type == ResourceType.XP)
        {
            // player.GetComponent<PlayerStats>().AddXP(resourceValue);
            Debug.Log($"Gained {resourceValue} XP!");
        }
        else if (type == ResourceType.Currency)
        {
            // player.GetComponent<PlayerInventory>().AddCurrency(resourceValue);
            Debug.Log($"Gained {resourceValue} Currency!");
        }

        // 2. Return to pool
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
