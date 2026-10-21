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
            // TODO: Hook up to PlayerStats or LevelManager later
            // player.GetComponent<PlayerStats>().AddXP(resourceValue);
            Debug.Log($"Gained {resourceValue} XP!");
        }
        else if (type == ResourceType.Currency)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCurrency(resourceValue);
            }
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
