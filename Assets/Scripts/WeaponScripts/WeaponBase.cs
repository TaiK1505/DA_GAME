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
    private float nextAltFireTime; 
    
    private Rigidbody2D playerRb; 

    private void Awake()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    private void Start()
    {
        if (weaponData != null)
        {
            currentAmmo = weaponData.magazineSize;
            currentReserve = weaponData.maxReserveAmmo;
        }
    }

    private void OnEnable()
    {
        isReloading = false; 
        UpdateAmmoUI();
    }

    public void StartShooting() { isShooting = true; }
    public void StopShooting() { isShooting = false; }
    
    private void Update()
    {
        // 1. UPDATE ALT-FIRE UI
        if (weaponData != null && weaponData.hasAltFire)
        {
            if (Time.time < nextAltFireTime)
            {
                float timeRemaining = nextAltFireTime - Time.time;
                float timePassed = weaponData.altFireCooldown - timeRemaining;
                
                float fillPercentage = timePassed / weaponData.altFireCooldown;
                if (EquipmentUI.instance != null) EquipmentUI.instance.UpdateAltFireUI(fillPercentage);
            }
            else
            {
                if (EquipmentUI.instance != null) EquipmentUI.instance.UpdateAltFireUI(1f);
            }
        }

        // 2. PRIMARY FIRE LOGIC
        if (isReloading) return;

        if (isShooting && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0 && !weaponData.bottomlessClip)
            {
                StartCoroutine(ReloadRoutine());
                return; 
            }

            Shoot();
            nextFireTime = Time.time + weaponData.fireRate;
        }
    }
    
    protected virtual void Shoot()
    {
        FireSpread(weaponData.bulletPrefab, weaponData.projectilesPerShot, weaponData.spreadAngle, weaponData.damage);
        
        ApplyRecoil(weaponData.baseRecoilForce);
        TriggerScreenShake(weaponData.baseShakeDuration, weaponData.baseScreenShake); // Passes Duration AND Intensity

        if (!weaponData.bottomlessClip)
        {
            currentAmmo--;
            UpdateAmmoUI();
            if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
        }
    }

    public virtual void AltFire()
    {
        if (!weaponData.hasAltFire || Time.time < nextAltFireTime) 
            return;

        GameObject prefabToFire = weaponData.altFireBulletPrefab != null ? weaponData.altFireBulletPrefab : weaponData.bulletPrefab;

        FireSpread(prefabToFire, weaponData.altFireProjectileCount, weaponData.altFireSpreadAngle, weaponData.damage * weaponData.altFireDamageMultiplier);
        
        ApplyRecoil(weaponData.altFireRecoilForce);
        TriggerScreenShake(weaponData.altFireShakeDuration, weaponData.altFireScreenShake); // Passes Duration AND Intensity

        nextAltFireTime = Time.time + weaponData.altFireCooldown;
    }

    private void FireSpread(GameObject prefabToSpawn, int count, float spread, float dmg)
    {
        for (int i = 0; i < count; i++)
        {
            // ---> THE FIX: Natural random spread instead of a perfect fan <---
            float randomAngle = count > 1 ? Random.Range(-spread / 2f, spread / 2f) : 0f;
            
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, randomAngle);
            
            GameObject bullet = ObjectPoolManager.Instance.SpawnObject(prefabToSpawn, firePoint.position, bulletRotation);
            
            ProjectileScript proj = bullet.GetComponent<ProjectileScript>();
            if (proj != null) proj.damage = dmg;
        }
    }

    private void ApplyRecoil(float force)
    {
        if (force <= 0f || playerRb == null) return;

        Vector2 recoilDir = -firePoint.right;
        float finalForce = force;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, weaponData.pointBlankDetectionRange, weaponData.recoilDetectionLayers);
        
        if (hit.collider != null)
        {
            // MULTIPLIER TRIGGERED (Point-blank into wall or enemy)
            finalForce *= weaponData.wallKickbackMultiplier;
            
            PlayerController player = GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.ApplyWeaponRecoil(recoilDir, finalForce);
            }
        }
        else
        {
            // NORMAL BEHAVIOR (Shooting in the open)
            playerRb.AddForce(recoilDir * finalForce, ForceMode2D.Impulse);
        }
    }

    private void TriggerScreenShake(float duration, float intensity)
    {
        if (intensity <= 0f || duration <= 0f) return;
        
        if (CameraController.Instance != null)
        {
            CameraController.Instance.TriggerShake(duration, intensity);
        }
    }

    public IEnumerator ReloadRoutine()
    {
        if (currentAmmo == weaponData.magazineSize || (!weaponData.infiniteReserveAmmo && currentReserve <= 0))
            yield break;

        isReloading = true;

        yield return new WaitForSeconds(weaponData.reloadTime);

        int bulletsNeeded = weaponData.magazineSize - currentAmmo;

        if (weaponData.infiniteReserveAmmo)
        {
            currentAmmo = weaponData.magazineSize;
        }
        else
        {
            int bulletsToLoad = Mathf.Min(bulletsNeeded, currentReserve);
            currentAmmo += bulletsToLoad;
            currentReserve -= bulletsToLoad;
        }

        isReloading = false;
        UpdateAmmoUI(); 
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
        if (isReloading || weaponData.bottomlessClip) return;
        if (currentAmmo == weaponData.magazineSize) return;
        if (!weaponData.infiniteReserveAmmo && currentReserve <= 0) return;

        StartCoroutine(ReloadRoutine());
    }
}