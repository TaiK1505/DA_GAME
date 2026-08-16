using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(HealthComponent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyStats;

    [Header("Physics States")]
    private float controlRegainTimer = 0f;
    private bool isKnockedBack = false;
    private bool isStunned = false;
    
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private HealthComponent healthComponent;
    private Rigidbody2D rb;
    private void Awake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        healthComponent = GetComponent<HealthComponent>();
        rb = GetComponent<Rigidbody2D>();
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
            aiPath.endReachedDistance = 0.1f; 
            healthComponent.InitializeHealth(enemyStats.maxHealth);
        }
        else
        {
            Debug.LogWarning("Enemy spawned but has no EnemyData assigned!");
        }
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        isKnockedBack = true;
        controlRegainTimer = duration;
        
        // Turn off the A* brain completely 
        if (aiPath != null)
        {
            aiPath.enabled = false; 
        }
        
        // the pure physics engine blast them away
        if (rb != null)
        {
            rb.linearVelocity = force;
        }
    }

    private void Update()
    {
        // Only run this timer if we are currently flying through the air
        if (isKnockedBack || isStunned)
        {
            controlRegainTimer -= Time.deltaTime;

            // When the stun duration is over...
            if (controlRegainTimer <= 0f)
            {
                isKnockedBack = false;
                isStunned = false;
                
                // Wake the A* brain back up so the enemy resumes chasing you!
                if (aiPath != null)
                {
                    aiPath.enabled = true;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isKnockedBack && collision.gameObject.CompareTag("Wall")) 
        {
            // THE WALL SPLAT!
            isKnockedBack = false;       // We are no longer flying
            isStunned = true;            // We are now fully Stunned
            controlRegainTimer = 2.0f;  

            // Stop the sliding physics instantly so they don't slide up the wall
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; 
            }

            Debug.Log("Enemy Splatted against a Wall!");
        }
    }
}
