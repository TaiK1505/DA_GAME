using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Weapon Settings")]
    [Tooltip("Drag the complete Weapon Prefab (e.g., Shotgun) here!")]
    public GameObject weaponPrefab;

    public void Interact(GameObject player)
    {
        if (weaponPrefab == null) return;

        ArsenalManager arsenal = player.GetComponent<ArsenalManager>();
        
        if (arsenal != null)
        {
            // Tell the Arsenal to build and equip the weapon
            arsenal.SpawnAndPickupGun(weaponPrefab);
            
            Debug.Log("Player picked up: " + weaponPrefab.name);

            // Destroy the pickup in the world so you can't grab it twice
            Destroy(gameObject); 
        }
    }
}
