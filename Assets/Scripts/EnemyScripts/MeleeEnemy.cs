using UnityEngine;
using Pathfinding;

public class MeleeEnemy : MonoBehaviour
{
    
    [Header("Enemy Data")]
    public EnemyData enemyStats;

    private EnemyAI myBaseAI;
    private Transform player;
    private AIPath aiPath;
    private Rigidbody2D rb;
    
    // The Brain States
    private enum AttackState { Chasing, WindingUp, Attacking, Cooldown }
    private AttackState currentState = AttackState.Chasing;
    private float stateTimer = 0f;

    private float originalMass;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        myBaseAI = GetComponent<EnemyAI>();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            originalMass = rb.mass;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) 
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private void Update()
    {
        // 1. THE INTERRUPT
        if (myBaseAI != null && (myBaseAI.isKnockedBack || myBaseAI.isStunned))
        {
            ResetToChase();
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. THE STATE MACHINE
        switch (currentState)
        {
            case AttackState.Chasing:
                aiPath.canMove = true;
                
                // Pull from enemyStats!
                if (distanceToPlayer <= enemyStats.attackRange)
                {
                    currentState = AttackState.WindingUp;
                    stateTimer = enemyStats.windupTime;
                    aiPath.canMove = false; 
                    rb.linearVelocity = Vector2.zero;

                    if (spriteRenderer != null) spriteRenderer.color = Color.red;
                    
                    // ANTI-BULLDOZE: Make them a brick wall while winding up
                    rb.mass = 1000f; 
                }
                break;

            case AttackState.WindingUp:
                stateTimer -= Time.deltaTime;
                
                // Force velocity to zero so they don't slide if the player bumps them
                rb.linearVelocity = Vector2.zero; 

                if (stateTimer <= 0)
                {
                    currentState = AttackState.Attacking;
                }
                break;

            case AttackState.Attacking:
                // Return to normal weight so your weapons can knock them back again
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
                rb.mass = originalMass; 

                if (distanceToPlayer <= enemyStats.attackRange + 0.5f) 
                {
                    HealthComponent playerHealth = player.GetComponent<HealthComponent>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(enemyStats.damageToPlayer);
                        Debug.Log($"Enemy bit the Player for {enemyStats.damageToPlayer} damage!");
                    }
                }
                
                currentState = AttackState.Cooldown;
                stateTimer = enemyStats.cooldownTime;
                break;

            case AttackState.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    ResetToChase();
                }
                break;
        }
    }

    private void ResetToChase()
    {
        currentState = AttackState.Chasing;
        if (aiPath != null) aiPath.canMove = true;
        if (rb != null) rb.mass = originalMass; // Failsafe reset

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }
}
