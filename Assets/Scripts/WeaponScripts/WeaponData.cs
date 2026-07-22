using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon ID")]
    public string weaponName;
    
    [Header("Combat Stats")]
    public float damage = 10f;
    public float fireRate = 0.2f; // Time in seconds between shots
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    
    [Header("Scaling (Phase 5 Prep)")]
    // We will expand on this when we build the RPG stats later!
    public string scalesOffStat = "None"; 

    [Header("Prefabs")]
    public GameObject bulletPrefab;
}
