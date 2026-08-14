using UnityEngine;

[CreateAssetMenu(fileName = "New Burst Grenade", menuName = "Gadgets/Burst Grenade")]
public class BurstGrenadeData : GadgetData
{
    [Header("Explosion Stats")]
    public GameObject grenadePrefab;
    public float blastRadius = 4f;
    public float blastForce = 60f;
    public float enemyKnockbackDuration = 0.8f;
    public float detonationDelay = 0.2f;
    public float burstGrenadeBoostFriction = 0.8f;

    [Header("Throw Stats")]
    public float maxThrowRange = 6f; 
    public float throwSpeed = 15f;    
}
