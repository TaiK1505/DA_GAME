using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy ID")]
    public string enemyName = "Basic Enemy";
    
    [Header("Health")]
    public float maxHealth = 100f; 

    [Header("Movement ")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f; // 0.5 for Melee, 5 for Ranged

    [Header("Combat")]
    public float damageToPlayer = 10f;
}
