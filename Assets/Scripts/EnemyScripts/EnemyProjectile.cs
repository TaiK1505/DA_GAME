using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Flight Stats")]
    public float speed = 15f; // Slightly slower than player bullets!
    public float lifetime = 3f;
   
   [HideInInspector] public float damage;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // Shoots forward exactly like the player bullet
        rb.linearVelocity = transform.right * speed;
       
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        // Safety cleanup for your Object Pool
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TARGET CHECK 1: Did we hit the Player?
        if (collision.CompareTag("Player"))
        {
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(damage);
            }   
            
            Debug.Log("Sniper hit the Player!");
            Deactivate();
        }
        // TARGET CHECK 2: Did we hit a Wall?
        else if (collision.CompareTag("Wall"))
        {
            Deactivate();
        }
        
        
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
