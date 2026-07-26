using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    
    [Header("Flight Stats")]
    public float speed = 25f;
    public float lifetime = 3f;

    [HideInInspector] public float damage; 

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
         rb.linearVelocity = transform.right * speed;

       
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        // Safety cleanup for our future Object Pool
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When it hits anything, print a message and remove the bullet.
        Debug.Log("Bullet hit: " + collision.name);
        Deactivate();
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
