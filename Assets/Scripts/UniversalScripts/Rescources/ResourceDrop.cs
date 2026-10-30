using UnityEngine;

public class ResourceDrop : Pickup
{
    public enum ResourceType { XP, Currency }
    
    [Header("Resource Settings")]
    public ResourceType type;
    public int resourceValue = 10;

    protected override void OnCollected(GameObject player)
    {
        if (type == ResourceType.XP)
        {
            LevelingManager levelManager = player.GetComponent<LevelingManager>();
            if (levelManager != null)
            {
                levelManager.AddXP(resourceValue);
            }
        }
        else if (type == ResourceType.Currency)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCurrency(resourceValue);
            }
        }

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
