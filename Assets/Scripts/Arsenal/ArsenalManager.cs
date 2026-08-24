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
        if (isMeleeActive) 
        {
            ToggleMelee(true, false); 
            return; 
        }

        if (gunInventory.Count <= 1) return; 

        currentGunIndex = (currentGunIndex + 1) % gunInventory.Count;
        EquipGun(currentGunIndex);
    }

    public void CyclePrevious()
    {
        if (isMeleeActive) 
        {
            ToggleMelee(true, false);
            return;
        }

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
            
            // ---> NEW: Tell the HUD we swapped guns! <---
            UpdateWeaponUI(gunInventory[currentGunIndex]);
        }
    }

    private void HideAllGuns()
    {
        ReleaseTrigger(); 

        foreach (GameObject gun in gunInventory)
        {
            if (gun != null) gun.SetActive(false);
        }
    }

    // THE MELEE TOGGLE & TRAVERSAL BUFF

    public void ToggleMelee(bool forceState = false, bool targetState = false)
    {
        isMeleeActive = forceState ? targetState : !isMeleeActive;

        if (isMeleeActive)
        {
            HideAllGuns();
            if (meleeWeapon != null) 
            {
                meleeWeapon.SetActive(true);
                // ---> NEW: Tell the HUD we pulled out the Sword! <---
                UpdateWeaponUI(meleeWeapon);
            }
            
            Debug.Log("KATANA EQUIPPED: +15% Traversal Speed Active!");
        }
        else
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(false);
            EquipGun(currentGunIndex);

            Debug.Log("GUN EQUIPPED: Combat Speed Normal.");
        }
    }

    // INVENTORY MANAGEMENT & SPAWNING

    public void SpawnAndPickupGun(GameObject gunPrefab)
    {
        GameObject newGun = Instantiate(gunPrefab, weaponPivot);
        
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
            gunInventory[currentGunIndex] = newGun;
            EquipGun(currentGunIndex);
        }
    }

    public void UpgradeWeaponCapacity(int extraSlots)
    {
        maxGunCapacity += extraSlots;
    }

    // COMBAT LOGIC
    public void PullTrigger()
    {
        if (isMeleeActive)
        {
            if (meleeWeapon != null)
            {
                MeleeWeaponController meleeScript = meleeWeapon.GetComponent<MeleeWeaponController>();
                if (meleeScript != null) meleeScript.Swing();
            }
            return;
        }

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
            MeleeWeaponController meleeScript = meleeWeapon.GetComponent<MeleeWeaponController>();
            if (meleeScript != null) meleeScript.Throw();
        }
    }

    public void ReloadActiveWeapon()
    {
        // Katanas don't reload!
        if (isMeleeActive) return;

        // Tell the currently equipped gun to reload!
        if (gunInventory.Count > 0 && gunInventory[currentGunIndex] != null)
        {
            WeaponBase currentGun = gunInventory[currentGunIndex].GetComponent<WeaponBase>();
            if (currentGun != null)
            {
                currentGun.TryReload();
            }
        }
    }

    
    private void UpdateWeaponUI(GameObject activeWeapon)
    {
        if (EquipmentUI.instance == null || activeWeapon == null) return;

        SpriteRenderer sr = activeWeapon.GetComponentInChildren<SpriteRenderer>();
        
        // This controls if the left UI box opens up. 
        // (Later, when guns get alt-fires, we will update this line to check for them too!)
        bool hasAltFire = activeWeapon.GetComponent<MeleeWeaponController>() != null;

        if (sr != null)
        {
            EquipmentUI.instance.EquipWeapon(sr.sprite, sr.color, hasAltFire);
        }
        else
        {
            EquipmentUI.instance.EquipWeapon(null, Color.gray, hasAltFire);
        }

        if (isMeleeActive)
        {
            EquipmentUI.instance.UpdateAmmoUI(-1, 0, false, false); 
        }
    }
}
