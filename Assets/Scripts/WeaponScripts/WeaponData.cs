using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon ID")]
    public string weaponName;
    
    [Header("Combat Stats")]
    public float damage = 10f;
    public float fireRate = 0.2f; // Time in seconds between shots
    public int maxAmmo = 30;

    [Header("Shotgun & Spread")]
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f; // Set to 45 for a wide shotgun fan

    [Header("Recoil & Movement")]
    public float baseRecoilForce = 0f; 
    public float wallKickbackMultiplier = 2.5f; // Amplifies the push if fired against a wall/enemy
    public float pointBlankDetectionRange = 1.5f; // How close you must be to trigger the massive pushback
    public LayerMask recoilDetectionLayers; // Set this to Environment/Enemies

    [Header("Ranged Alt-Fire")]
    public bool hasAltFire = false;
    public GameObject altFireBulletPrefab;
    public int altFireProjectileCount = 1; // Shoot a single heavy slug
    public float altFireSpreadAngle = 0f;  // Keep it tight
    public float altFireDamageMultiplier = 2f; 
    public float altFireRecoilForce = 15f; // Massive base pushback
    public int altFireAmmoCost = 2;
    public float altFireCooldown = 1f;

    [Header("Screen Shake (Visual Recoil)")]
    public float baseScreenShake = 2f;    
    public float baseShakeDuration = 0.05f; // Short, punchy fraction of a second
    
    public float altFireScreenShake = 5f; 
    public float altFireShakeDuration = 0.08f;
    
    [Header("Scaling (Phase 5 Prep)")]
    
    public string scalesOffStat = "None"; 

    [Header("Prefabs")]
    public GameObject bulletPrefab;

    [Header("Ammo & Reloading")]
    public int magazineSize = 30;       
    public int maxReserveAmmo = 300;    
    public float reloadTime = 1.5f;     
    
    [Header("Special Ammo Rules")]
    public bool infiniteReserveAmmo = false; 
    public bool bottomlessClip = false;
}
