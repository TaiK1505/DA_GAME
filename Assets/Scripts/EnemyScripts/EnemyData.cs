using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy ID")]
    public string enemyName = "Basic Enemy";
    
    [Header("Health")]
    public float maxHealth = 100f; 

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f; 

    [Header("Combat Core")]
    public float damageToPlayer = 10f;
    public float attackRate = 1f;

    [Header("State Machine Combat")]
    public float attackRange = 1.5f;
    public float windupTime = 0.4f;
    public float cooldownTime = 1.0f;
    public float dashForce = 12f;
}
