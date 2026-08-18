using UnityEngine;
using System.Collections.Generic;

public class ArsenalManager : MonoBehaviour
{
    [Header("Component Links")]
    public PlayerController player;
    public Transform weaponPivot; // Where the weapons actually spawn/live!

    [Header("The Dedicated Melee")]
    public GameObject meleeWeapon; // Your Katana Dummy goes here
    public bool isMeleeActive = false;
    public float meleeSpeedBuff = 1.15f; 

    [Header("The Ranged Arsenal")]
    public int maxGunCapacity = 2; 
    public List<GameObject> gunInventory = new List<GameObject>();
    public int currentGunIndex = 0;
    
    
    public GameObject starterGunPrefab; // Your real working gun prefab!

    void Start()
    {
        // Auto-link the player script if forgotten
        if (player == null) player = GetComponent<PlayerController>();

        // If we assigned a starter gun, spawn it on the shoulder!
        if (starterGunPrefab != null)
        {
            SpawnAndPickupGun(starterGunPrefab);
        }
        else if (gunInventory.Count > 0)
        {
            EquipGun(0);
        }
    }

    // SWAPPING LOGIC (Remap-Friendly)

    public void CycleNext()
    {
        if (gunInventory.Count <= 1) return; // Need at least 2 guns to cycle
        if (isMeleeActive) ToggleMelee(true, false); // Put away the sword if it's out!

        currentGunIndex = (currentGunIndex + 1) % gunInventory.Count;
        EquipGun(currentGunIndex);
    }

    public void CyclePrevious()
    {
        if (gunInventory.Count <= 1) return; 
        if (isMeleeActive) ToggleMelee(true, false);

        currentGunIndex--;
        if (currentGunIndex < 0) currentGunIndex = gunInventory.Count - 1;
        EquipGun(currentGunIndex);
    }

    public void EquipGun(int index)
    {
        HideAllGuns();

        currentGunIndex = index;
        if (gunInventory[currentGunIndex] != null)
        {
            gunInventory[currentGunIndex].SetActive(true);
        }
    }

    private void HideAllGuns()
    {
        // 1. Force the current gun to stop shooting before we put it away!
        ReleaseTrigger(); 

        // 2. Put them all away
        foreach (GameObject gun in gunInventory)
        {
            if (gun != null) gun.SetActive(false);
        }
    }

    // THE MELEE TOGGLE & TRAVERSAL BUFF

    public void ToggleMelee(bool forceState = false, bool targetState = false)
    {
        // Allow us to forcefully turn it off, or just naturally toggle it
        isMeleeActive = forceState ? targetState : !isMeleeActive;

        if (isMeleeActive)
        {
            HideAllGuns();
            if (meleeWeapon != null) meleeWeapon.SetActive(true);
            
            // TODO: player.ApplyMeleeSpeedBuff(meleeSpeedBuff); 
            Debug.Log("KATANA EQUIPPED: +15% Traversal Speed Active!");
        }
        else
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(false);
            EquipGun(currentGunIndex);

            // TODO: player.RemoveMeleeSpeedBuff();
            Debug.Log("GUN EQUIPPED: Combat Speed Normal.");
        }
    }

    // INVENTORY MANAGEMENT & SPAWNING

    public void SpawnAndPickupGun(GameObject gunPrefab)
    {
        // Spawn the gun and parent it to the invisible aiming pivot
        GameObject newGun = Instantiate(gunPrefab, weaponPivot);
        
        // Zero out its transform so it orbits perfectly
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.localRotation = Quaternion.identity;

        TryPickupGun(newGun);
    }

    public void TryPickupGun(GameObject newGun)
    {
        if (gunInventory.Count < maxGunCapacity)
        {
            gunInventory.Add(newGun);
            EquipGun(gunInventory.Count - 1);
        }
        else
        {
            Debug.Log("Dropping old gun: " + gunInventory[currentGunIndex].name);
            // TODO: Actually drop the weapon on the floor here later
            
            gunInventory[currentGunIndex] = newGun;
            EquipGun(currentGunIndex);
        }
    }

    public void UpgradeWeaponCapacity(int extraSlots)
    {
        maxGunCapacity += extraSlots;
        Debug.Log("CYBERWARE UPGRADE: Arsenal capacity increased to " + maxGunCapacity);
    }

    // COMBAT LOGIC
    public void PullTrigger()
    {
        if (isMeleeActive)
        {
            Debug.Log("Swinging the Katana!");
            // TODO: Katana slash logic!
            return;
        }

        // Tell the currently equipped gun to shoot!
        if (gunInventory.Count > 0 && gunInventory[currentGunIndex] != null)
        {
            WeaponBase currentGun = gunInventory[currentGunIndex].GetComponent<WeaponBase>();
            if (currentGun != null) currentGun.StartShooting();
        }
    }

    public void ReleaseTrigger()
    {
        if (isMeleeActive) return;

        if (gunInventory.Count > 0 && gunInventory[currentGunIndex] != null)
        {
            WeaponBase currentGun = gunInventory[currentGunIndex].GetComponent<WeaponBase>();
            if (currentGun != null) currentGun.StopShooting();
        }
    }
}
