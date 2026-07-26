using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    
    public WeaponData weaponData;
    public Transform firePoint; 

    
    private bool isShooting;
    private float nextFireTime;
    
    
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
        // Check if the mouse is held down AND if our fire rate cooldown is finished
        if (isShooting && Time.time >= nextFireTime)
        {
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
    }
}
