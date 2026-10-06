using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(HealthComponent))]
public class EnemyAI : MonoBehaviour, IStunnable
{
    // ---> The Official State Machine <---
    public enum EnemyState { Chasing, KnockedBack, Stunned }

    [Header("Enemy Data")]
    public EnemyData enemyStats;

    [Header("State Machine")]
    public EnemyState currentState = EnemyState.Chasing;
    private float controlRegainTimer = 0f;

    [Header("Combat Reactions")]
    [Tooltip("1.0 = Normal. 1.3 = Extra push. 0.2 = Heavy brute.")]
    public float knockbackMultiplier = 1f;
    
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
        if (playerObject != null) destinationSetter.target = playerObject.transform;
        else Debug.LogWarning("Enemy spawned but couldn't find the Player!");

        if (enemyStats != null)
        {
            aiPath.maxSpeed = enemyStats.moveSpeed;
            aiPath.endReachedDistance = 0.1f; 
            healthComponent.InitializeHealth(enemyStats.maxHealth);
        }

        // ---> Reset memory for Object Pool <---
        currentState = EnemyState.Chasing;
        controlRegainTimer = 0f;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (aiPath != null) aiPath.enabled = true; 
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.Stunned)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        currentState = EnemyState.KnockedBack;
        controlRegainTimer = duration;
        
        if (aiPath != null) aiPath.enabled = false; 
        if (rb != null) rb.linearVelocity = force * knockbackMultiplier;
    }

    // ---> Clean getters for the other scripts! <---
    public bool IsCurrentlyStunned()
    {
        return currentState == EnemyState.Stunned;
    }

    public bool IsCurrentlyKnockedBack()
    {
        return currentState == EnemyState.KnockedBack;
    }

    public void Stun(float duration)
    {
        currentState = EnemyState.Stunned;
        controlRegainTimer = duration; 

        if (aiPath != null) aiPath.enabled = false; 
        if (rb != null) rb.linearVelocity = Vector2.zero; 
    }
    

    private void Update()
    {
        if (currentState == EnemyState.KnockedBack || currentState == EnemyState.Stunned)
        {
            controlRegainTimer -= Time.deltaTime;

            if (controlRegainTimer <= 0f)
            {
                currentState = EnemyState.Chasing; 
                
                if (aiPath != null) aiPath.enabled = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.KnockedBack && collision.gameObject.CompareTag("Wall")) 
        {
            currentState = EnemyState.Stunned; // Splat!
            controlRegainTimer = 2.0f;  

            if (rb != null) rb.linearVelocity = Vector2.zero; 
        }
    }
}