using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    
    [Header("Enemy Data")]
    public EnemyData enemyStats;

    private float nextAttackTime = 0f;
    private void OnCollisionStay2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextAttackTime)
            {
                HealthComponent playerHealth = collision.gameObject.GetComponent<HealthComponent>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(enemyStats.damageToPlayer);
                   
                    nextAttackTime = Time.time + enemyStats.attackRate;

                    Debug.Log($"Enemy bit the Player for {enemyStats.damageToPlayer} damage!");
                }
            }
        }
    }
}
