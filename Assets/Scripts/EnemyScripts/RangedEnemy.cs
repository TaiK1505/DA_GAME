using UnityEngine;
using Pathfinding;

public class RangedEnemy : MonoBehaviour
{
    
    [Header("Enemy Data")]
    public EnemyData enemyStats;
   
    [Header("Shooting")]
    public GameObject enemyBulletPrefab; 
    public Transform firePoint;          
    public Transform gunPivot;

    [Header("Line of Sight")]
    public float attackRange = 8f;
    public LayerMask obstacleLayer;
    public AIPath aiPath;   

    public AIDestinationSetter destinationSetter;
    public HealthComponent healthComponent;

    private Transform player;
    private float nextAttackTime = 0f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    
    // 1. ADD THE REFERENCE TO YOUR BASE AI
    private EnemyAI myBaseAI;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // 2. GRAB THE SCRIPT ON WAKE UP
        myBaseAI = GetComponent<EnemyAI>();
    }
    
    private void OnEnable()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            destinationSetter.target = playerObject.transform; 
            player = playerObject.transform; 
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

    private void Update()
    {
        // 3. THE OFF SWITCH!
        // If they are flying through the air or stunned against a wall, completely skip this Update loop!
        if (myBaseAI != null && (myBaseAI.isKnockedBack || myBaseAI.isStunned))
        {
            return; 
        }

        // --- Normal Ranged Logic below this line ---

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;  
        }
        else
        {
            spriteRenderer.flipX = false; 
        }

        Vector2 aimDirection = (player.position - gunPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

        //The Line of Sight Raycast 
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        
        bool hasLineOfSight = (hit.collider == null); 

        if (distanceToPlayer <= enemyStats.stoppingDistance && hasLineOfSight)
        {
            aiPath.canMove = false; 
            rb.linearVelocity = Vector2.zero; 
            rb.angularVelocity = 0f; 
        }
        else
        {
            aiPath.canMove = true;
        }

        if (distanceToPlayer <= attackRange && hasLineOfSight)
        {
            if (Time.time >= nextAttackTime)
            {
                Shoot();
                nextAttackTime = Time.time + enemyStats.attackRate;
            }
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        Vector2 aimDirection = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        GameObject spawnedBullet = ObjectPoolManager.Instance.SpawnObject(enemyBulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        ProjectileScript projectile = spawnedBullet.GetComponent<ProjectileScript>();
        
        if (projectile != null)
        {
            projectile.damage = enemyStats.damageToPlayer;
        }
    }
}
