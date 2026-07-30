using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    
    [Header("Enemy Data")]
    public EnemyData enemyStats;
   
   [Header("Shooting")]
    public GameObject enemyBulletPrefab; // The bullet it will shoot
    public Transform firePoint;          // Where the bullet spawns (tip of the gun)
    public Transform gunPivot;

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
        if (playerObject != null) player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null) return;

        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;  // Player is to the left, look left!
        }
        else
        {
            spriteRenderer.flipX = false; // Player is to the right, look right!
        }

        // 4. Rotate the invisible shoulder to aim the gun
        Vector2 aimDirection = (player.position - gunPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

       
        if (distanceToPlayer <= enemyStats.stoppingDistance + 1f)
        {
            // 3. Is our gun reloaded?
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
