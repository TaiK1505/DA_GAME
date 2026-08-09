using UnityEngine;
using Pathfinding;

public class RangedEnemy : MonoBehaviour
{
    
    [Header("Enemy Data")]
    public EnemyData enemyStats;
   
   [Header("Shooting")]
    public GameObject enemyBulletPrefab; // The bullet it will shoot
    public Transform firePoint;          // Where the bullet spawns (tip of the gun)
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

    private void Awake()
    {
        // 2. We grab the Sprite Renderer off the yellow square
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void OnEnable()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

    if (playerObject != null)
    {
        // 1. Give the target to the A* Legs
        destinationSetter.target = playerObject.transform; 
        
        // 2. Give the target to the Raycast Brain! (THE MISSING LINK)
        player = playerObject.transform; 
    }
    else
    {
        Debug.LogWarning("Enemy spawned but couldn't find the Player!");
    }

    if (enemyStats != null)
    {
        aiPath.maxSpeed = enemyStats.moveSpeed;
        
        // THE MANUAL TRANSMISSION FIX: Turn off the A* auto-brakes
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
    
    // True if the laser hits nothing 
    bool hasLineOfSight = (hit.collider == null); 

    if (distanceToPlayer <= enemyStats.stoppingDistance && hasLineOfSight)
    {
        aiPath.canMove = false;
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

        // Calculate the math angle to look directly at the player
        Vector2 aimDirection = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        GameObject spawnedBullet = ObjectPoolManager.Instance.SpawnObject(enemyBulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        EnemyProjectile enemyProjectile = spawnedBullet.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.damage = enemyStats.damageToPlayer;
        }
    }
}
