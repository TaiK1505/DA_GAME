using UnityEngine;
using System.Collections.Generic;

public class ArsenalManager : MonoBehaviour
{
    [Header("Component Links")]
    public PlayerController player;
    public Transform weaponPivot; // Where the weapons actually spawn/live!

    [Header("The Dedicated Melee")]
    public GameObject starterMeleePrefab;
    public GameObject meleeWeapon; 
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

        if (starterMeleePrefab != null)
        {
            meleeWeapon = Instantiate(starterMeleePrefab, weaponPivot);
            meleeWeapon.transform.localPosition = Vector3.zero;
            meleeWeapon.transform.localRotation = Quaternion.identity;
            
            // Force it to be holstered when the game starts
            meleeWeapon.SetActive(false); 
        }

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
        // 1. If the Katana is out, scrolling ALWAYS puts it away and brings out your last gun.
        if (isMeleeActive) 
        {
            ToggleMelee(true, false); 
            return; // We successfully swapped back to the gun, so stop reading code here!
        }

        // 2. If we are already holding a gun, we need at least 2 guns to cycle to a new one.
        if (gunInventory.Count <= 1) return; 

        currentGunIndex = (currentGunIndex + 1) % gunInventory.Count;
        EquipGun(currentGunIndex);
    }

    public void CyclePrevious()
    {
        // 1. Always check the Katana first!
        if (isMeleeActive) 
        {
            ToggleMelee(true, false);
            return;
        }

        // 2. Need at least 2 guns to cycle.
        if (gunInventory.Count <= 1) return; 

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
            // Find the Katana script on our dummy and tell it to swing!
            if (meleeWeapon != null)
            {
                Katana katanaScript = meleeWeapon.GetComponent<Katana>();
                if (katanaScript != null) katanaScript.Swing();
            }
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

    public void AltFire()
    {
        if (isMeleeActive && meleeWeapon != null)
        {
            Katana katanaScript = meleeWeapon.GetComponent<Katana>();
            if (katanaScript != null) katanaScript.Throw();
        }
    }
}
