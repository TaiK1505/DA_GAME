using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Game Data/Melee Weapon Data")]
public class MeleeWeaponData : ScriptableObject
{
    [Header("Weapon ID")]
    public string weaponName;
    
    [Header("Combat Stats")]
    public float damage = 25f;
    public float attackCooldown = 0.3f; // Fast swings!
    public float attackRange = 1.5f;    // How wide the phantom hitbox is
   
    
    [Header("Base Lunge")]
    public bool canLunge = true;
    public float lungeDistance = 2.5f;   // How close they must be to trigger normal lunge
    public float lungeDuration = 0.05f;  // Instant micro-step!

    [Header("Enhanced Lunge")]
    public float executionRangeMultiplier = 4f; // 2.5 range becomes 10 range!
    public float executionDuration = 0.15f;     // A slightly longer, dramatic dash across the room

    [Header("Throw Mechanics")]
    public bool canThrow = true;
    public float throwDamage = 10f;
    public float stunDuration = 2f;
    public float maxThrowDistance = 8f;   
    public float throwSpeed = 25f;        
    public float throwSpinSpeed = 1500f;  
    public float throwHitboxRadius = 1f; 
    public float throwCooldown = 1.5f;
    public GameObject dummyPrefab;
    
    
    [Header("Scaling (Phase 5 Prep)")]
    public string scalesOffStat = "None"; 

    [Header("Visuals")]
    public GameObject slashVFXPrefab;
}
