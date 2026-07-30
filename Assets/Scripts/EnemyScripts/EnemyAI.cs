using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(HealthComponent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyStats;

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private HealthComponent healthComponent;
    private void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        healthComponent = GetComponent<HealthComponent>();
    }

   
    private void OnEnable()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            destinationSetter.target = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Enemy spawned but couldn't find the Player!");
        }

        if (enemyStats != null)
        {
            aiPath.maxSpeed = enemyStats.moveSpeed;
            aiPath.endReachedDistance = enemyStats.stoppingDistance;

            healthComponent.InitializeHealth(enemyStats.maxHealth);
        }
        else
        {
            Debug.LogWarning("Enemy spawned but has no EnemyData assigned!");
        }
    }
}
