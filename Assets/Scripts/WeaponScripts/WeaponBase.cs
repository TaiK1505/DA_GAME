using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;
    public Transform firePoint; 

    [Header("Runtime Ammo State")]
    public int currentAmmo;
    public int currentReserve;
    public bool isReloading = false;

    private bool isShooting;
    private float nextFireTime;
    
    private void Start()
    {
        // Start the game fully loaded!
        if (weaponData != null)
        {
            currentAmmo = weaponData.magazineSize;
            currentReserve = weaponData.maxReserveAmmo;
        }
    }

    private void OnEnable()
    {
        // Cancel any reloads if we swapped weapons mid-reload, and push our ammo to the UI
        isReloading = false; 
        UpdateAmmoUI();
    }

    public void StartShooting()
    {
        isShooting = true;
    }

    public void StopShooting()
    {
        isShooting = false;
    }
    
    private void Update()
    {
        // 1. If we are currently reloading, we absolutely cannot shoot!
        if (isReloading) return;

        // Check if the mouse is held down AND if our fire rate cooldown is finished
        if (isShooting && Time.time >= nextFireTime)
        {
            // 2. Are we empty? Force an auto-reload instead of shooting!
            if (currentAmmo <= 0 && !weaponData.bottomlessClip)
            {
                StartCoroutine(ReloadRoutine());
                return; 
            }

            Shoot();
            
            // Set the timer for the next allowed shot
            nextFireTime = Time.time + weaponData.fireRate;
        }
    }
    
    protected virtual void Shoot()
    {
        // 1. Spawn the physical bullet at the tip of the barrel
        GameObject bullet = ObjectPoolManager.Instance.SpawnObject(weaponData.bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 2. Give the bullet its damage number from our ScriptableObject
        ProjectileScript projectileScript = bullet.GetComponent<ProjectileScript>();
        if (projectileScript != null)
        {
            projectileScript.damage = weaponData.damage;
        }

        // 3. DRAIN THE AMMO!
        if (!weaponData.bottomlessClip)
        {
            currentAmmo--;
            UpdateAmmoUI();

            // 4. Auto-Reload if we just fired the absolute last bullet in the clip
            if (currentAmmo <= 0)
            {
                StartCoroutine(ReloadRoutine());
            }
        }
    }

    // ---> NEW: THE RELOAD MATH <---
    public IEnumerator ReloadRoutine()
    {
        // Safety check: Can't reload if we are already full, or if we have no backpack ammo left!
        if (currentAmmo == weaponData.magazineSize || (!weaponData.infiniteReserveAmmo && currentReserve <= 0))
            yield break;

        isReloading = true;
        Debug.Log("Reloading...");

        // Wait for the exact time you set in the Inspector
        yield return new WaitForSeconds(weaponData.reloadTime);

        // How many bullets are we missing?
        int bulletsNeeded = weaponData.magazineSize - currentAmmo;

        if (weaponData.infiniteReserveAmmo)
        {
            // Starter Gun: Magically refill the clip!
            currentAmmo = weaponData.magazineSize;
        }
        else
        {
            // Standard Gun: Take from backpack, but don't take more than we have!
            int bulletsToLoad = Mathf.Min(bulletsNeeded, currentReserve);
            currentAmmo += bulletsToLoad;
            currentReserve -= bulletsToLoad;
        }

        isReloading = false;
        UpdateAmmoUI(); // Push the new math to the screen!
        Debug.Log("Reload Complete!");
    }

    public void UpdateAmmoUI()
    {
        if (EquipmentUI.instance != null && weaponData != null)
        {
            EquipmentUI.instance.UpdateAmmoUI(currentAmmo, currentReserve, weaponData.infiniteReserveAmmo, weaponData.bottomlessClip);
        }
    }

    public void TryReload()
    {
        // 1. Can't reload if we are already reloading, or if the gun never reloads (Minigun)
        if (isReloading || weaponData.bottomlessClip) return;

        // 2. Can't reload if the magazine is already 100% full!
        if (currentAmmo == weaponData.magazineSize) return;

        // 3. Can't reload if we have 0 backpack ammo (and it's not the starter gun)
        if (!weaponData.infiniteReserveAmmo && currentReserve <= 0) return;

        // If we passed all checks, start the reload timer!
        StartCoroutine(ReloadRoutine());
    }
}
