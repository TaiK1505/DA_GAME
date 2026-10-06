using UnityEngine;
using Pathfinding;

public class RangedEnemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyStats;
    public HealthComponent healthComponent;

    [Header("Visuals")]
    public Color telegraphColor = Color.yellow; 

    [Header("Shooting")]
    public GameObject enemyBulletPrefab; 
    public Transform firePoint;          
    public Transform gunPivot;
    public float bulletSpread = 15f; 

    [Header("Line of Sight")]
    public LayerMask obstacleLayer;
    public AIPath aiPath;   

    private EnemyAI myBaseAI;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;

    private enum GunnerState { Chasing, WindingUp, Cooldown }
    private GunnerState currentState = GunnerState.Chasing;
    private float stateTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        myBaseAI = GetComponent<EnemyAI>();
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }
    
    private void OnEnable()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform; 
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (enemyStats != null)
        {
            aiPath.maxSpeed = enemyStats.moveSpeed;
            aiPath.endReachedDistance = 0.1f; 
            if (healthComponent != null) healthComponent.InitializeHealth(enemyStats.maxHealth);
        }
    }

    private void Update()
    {
        // ---> THE FIX <---
        if (myBaseAI != null && (myBaseAI.IsCurrentlyKnockedBack() || myBaseAI.IsCurrentlyStunned()))
        {
            ResetState();
            return; 
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        bool hasLineOfSight = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer).collider == null;

        FacePlayerAndAim();

        switch (currentState)
        {
            case GunnerState.Chasing:
                HandleMovement(distanceToPlayer, hasLineOfSight);
                if (distanceToPlayer <= enemyStats.attackRange && hasLineOfSight)
                {
                    currentState = GunnerState.WindingUp;
                    stateTimer = enemyStats.windupTime;
                    if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
                }
                break;

            case GunnerState.WindingUp:
                stateTimer -= Time.deltaTime;
                HandleMovement(distanceToPlayer, hasLineOfSight);

                if (stateTimer <= 0)
                {
                    Shoot();
                    currentState = GunnerState.Cooldown;
                    stateTimer = enemyStats.cooldownTime;
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                }
                break;

            case GunnerState.Cooldown:
                stateTimer -= Time.deltaTime;
                HandleMovement(distanceToPlayer, hasLineOfSight); 

                if (stateTimer <= 0) currentState = GunnerState.Chasing;
                break;
        }
    }

    private void HandleMovement(float distanceToPlayer, bool hasLineOfSight)
    {
        if (distanceToPlayer <= enemyStats.stoppingDistance && hasLineOfSight)
        {
            aiPath.canMove = false; 
            if (rb != null) rb.linearVelocity = Vector2.zero; 
        }
        else
        {
            aiPath.canMove = true;
            aiPath.destination = player.position;
        }
    }

    private void FacePlayerAndAim()
    {
        spriteRenderer.flipX = player.position.x < transform.position.x;
        if (gunPivot != null)
        {
            Vector2 aimDirection = (player.position - gunPivot.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            gunPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;
        
        Vector2 aimDirection = (player.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float randomSpread = Random.Range(-bulletSpread, bulletSpread);
        float finalAngle = baseAngle + randomSpread;

        GameObject spawnedBullet = ObjectPoolManager.Instance.SpawnObject(enemyBulletPrefab, firePoint.position, Quaternion.Euler(0, 0, finalAngle));
        ProjectileScript projectile = spawnedBullet.GetComponent<ProjectileScript>();
        if (projectile != null) projectile.damage = enemyStats.damageToPlayer;
    }

    private void ResetState()
    {
        currentState = GunnerState.Chasing;
        if (aiPath != null) aiPath.canMove = true;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }
}