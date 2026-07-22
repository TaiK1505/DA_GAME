using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    // This holds all our stats (Damage, Fire Rate, Bullet Prefab)
    public WeaponData weaponData;

    public Transform firePoint; 

    private PlayerControls controls;
    private bool isShooting;
    private float nextFireTime;
    
    private void Awake()
    {
        controls = new PlayerControls();
        
        // Listen to the left mouse button. 
        // We use started/canceled so automatic weapons keep firing while you hold it down!
        controls.Player.Fire.started += ctx => isShooting = true;
        controls.Player.Fire.canceled += ctx => isShooting = false;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
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

    // We use "virtual" so future laser/melee weapons can override this exact function!
    protected virtual void Shoot()
    {
        // 1. Spawn the physical bullet at the tip of the barrel
        // NOTE: We are using Instantiate purely to test if the math works right now.
        // We will replace this with the Object Pool Manager pull in the next step!
        GameObject bullet = Instantiate(weaponData.bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Give the bullet its damage number from our ScriptableObject
        ProjectileScript projectileScript = bullet.GetComponent<ProjectileScript>();
        if (projectileScript != null)
        {
            projectileScript.damage = weaponData.damage;
        }
    }
}
