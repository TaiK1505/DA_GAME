using UnityEngine;
using Pathfinding;

public class MeleeDasher : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyStats;
    public HealthComponent healthComponent;

    [Header("Visuals")]
    public Color telegraphColor = Color.blue;

    private EnemyAI myBaseAI;
    private Transform player;
    private AIPath aiPath;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private enum DasherState { Chasing, WindingUp, Dashing, Cooldown }
    private DasherState currentState = DasherState.Chasing;
    private float stateTimer = 0f;
    
    private Vector2 dashDirection;
    private float currentDashTime;

    private void Awake()
    {
        myBaseAI = GetComponent<EnemyAI>();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (enemyStats != null && healthComponent != null) healthComponent.InitializeHealth(enemyStats.maxHealth);
    }

    private void Update()
    {
        // ---> THE FIX: Cleanly checks the Global Brain <---
        if (myBaseAI != null && (myBaseAI.IsCurrentlyKnockedBack() || myBaseAI.IsCurrentlyStunned()))
        {
            ResetToChase();
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case DasherState.Chasing:
                aiPath.canMove = true;
                if (distanceToPlayer <= enemyStats.attackRange)
                {
                    currentState = DasherState.WindingUp;
                    stateTimer = enemyStats.windupTime;
                    aiPath.canMove = false;
                    if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
                }
                break;

            case DasherState.WindingUp:
                stateTimer -= Time.deltaTime;
                if (rb != null) rb.linearVelocity = Vector2.zero;

                if (stateTimer <= 0)
                {
                    dashDirection = (player.position - transform.position).normalized;
                    currentState = DasherState.Dashing;
                    currentDashTime = 0f;
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                }
                break;

            case DasherState.Dashing:
                currentDashTime += Time.deltaTime;
                if (rb != null) rb.linearVelocity = dashDirection * enemyStats.dashForce;

                if (currentDashTime >= enemyStats.maxDashDuration)
                {
                    StartCooldown();
                }
                break;

            case DasherState.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    ResetToChase();
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == DasherState.Dashing)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                HealthComponent playerHealth = collision.gameObject.GetComponent<HealthComponent>();
                if (playerHealth != null) playerHealth.TakeDamage(enemyStats.damageToPlayer);
                StartCooldown();
            }
            else if (collision.gameObject.CompareTag("Wall"))
            {
                StartCooldown();
            }
        }
    }

    private void StartCooldown()
    {
        currentState = DasherState.Cooldown;
        stateTimer = enemyStats.cooldownTime;
        if (rb != null) rb.linearVelocity = Vector2.zero; 
    }

    private void ResetToChase()
    {
        currentState = DasherState.Chasing;
        if (aiPath != null) aiPath.canMove = true;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }
}