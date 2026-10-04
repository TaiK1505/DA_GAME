using UnityEngine;
using Pathfinding;

public class SniperEnemy : MonoBehaviour
{
   [Header("Enemy Data")]
    public EnemyData enemyStats;
    public HealthComponent healthComponent;

    [Header("Visuals")]
    public Color telegraphColor = new Color(1f, 0.4f, 0f); // Deep Orange warning
    
    // ---> NEW: The Laser Sight! <---
    [Header("Laser Sight")]
    public LineRenderer laserLine;
    public float maxLaserDistance = 30f;

    [Header("Shooting")]
    public GameObject enemyBulletPrefab; 
    public Transform firePoint;          
    public Transform gunPivot;

    [Header("Line of Sight")]
    public LayerMask obstacleLayer; // The laser will use this to stop at walls!
    public AIPath aiPath;   

    private EnemyAI myBaseAI;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;

    private enum SniperState { Repositioning, Aiming, Reloading }
    private SniperState currentState = SniperState.Repositioning;
    private float stateTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        myBaseAI = GetComponent<EnemyAI>();
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        
        // Ensure laser is off when they spawn
        if (laserLine != null) laserLine.enabled = false;
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
        if (myBaseAI != null && (myBaseAI.isKnockedBack || myBaseAI.isStunned))
        {
            ResetState();
            return; 
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        bool hasLineOfSight = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer).collider == null;

        FacePlayerAndAim();
        
        // ---> NEW: Constantly draw the laser if aiming! <---
        HandleLaserSight();

        switch (currentState)
        {
            case SniperState.Repositioning:
                HandleMovement(distanceToPlayer, hasLineOfSight);

                if (distanceToPlayer <= enemyStats.attackRange && distanceToPlayer > enemyStats.retreatDistance && hasLineOfSight)
                {
                    currentState = SniperState.Aiming;
                    stateTimer = enemyStats.windupTime;
                    aiPath.canMove = false; 
                    
                    if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
                }
                break;

            case SniperState.Aiming:
                stateTimer -= Time.deltaTime;
                if (rb != null) rb.linearVelocity = Vector2.zero; 

                // If player breaks line of sight while aiming, cancel the shot!
                if (!hasLineOfSight)
                {
                    ResetState();
                    break;
                }

                if (stateTimer <= 0)
                {
                    Shoot();
                    currentState = SniperState.Reloading;
                    stateTimer = enemyStats.cooldownTime;
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                    
                    // Turn laser off when shot fires
                    if (laserLine != null) laserLine.enabled = false; 
                }
                break;

            case SniperState.Reloading:
                stateTimer -= Time.deltaTime;
                HandleMovement(distanceToPlayer, hasLineOfSight);

                if (stateTimer <= 0)
                {
                    currentState = SniperState.Repositioning;
                }
                break;
        }
    }

    // =========================================
    // ---> THE NEW LASER LOGIC <---
    // =========================================
    private void HandleLaserSight()
    {
        if (laserLine == null || firePoint == null) return;

        if (currentState == SniperState.Aiming)
        {
            laserLine.enabled = true;
            
            // Start the laser at the gun barrel
            laserLine.SetPosition(0, firePoint.position);

            Vector2 aimDirection = (player.position - firePoint.position).normalized;
            
            // Cast a ray out to see if a wall is in the way
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, aimDirection, maxLaserDistance, obstacleLayer);

            if (hit.collider != null)
            {
                // The laser hit a wall! Stop the red line exactly at the wall.
                laserLine.SetPosition(1, hit.point);
            }
            else
            {
                // No wall hit, shoot the laser far off-screen
                laserLine.SetPosition(1, (Vector2)firePoint.position + (aimDirection * maxLaserDistance));
            }
        }
        else
        {
            // Turn off the laser if they are reloading or repositioning
            laserLine.enabled = false;
        }
    }

    private void HandleMovement(float distanceToPlayer, bool hasLineOfSight)
    {
        if (distanceToPlayer < enemyStats.retreatDistance)
        {
            aiPath.canMove = true;
            Vector2 fleeDirection = (transform.position - player.position).normalized;
            aiPath.destination = (Vector2)transform.position + (fleeDirection * 5f);
        }
        else if (distanceToPlayer <= enemyStats.stoppingDistance && hasLineOfSight)
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
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        GameObject spawnedBullet = ObjectPoolManager.Instance.SpawnObject(enemyBulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        ProjectileScript projectile = spawnedBullet.GetComponent<ProjectileScript>();
        if (projectile != null) projectile.damage = enemyStats.damageToPlayer; 
    }

    private void ResetState()
    {
        currentState = SniperState.Repositioning;
        if (aiPath != null) aiPath.canMove = true;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        
        // Failsafe laser shutdown
        if (laserLine != null) laserLine.enabled = false;
    }
}
