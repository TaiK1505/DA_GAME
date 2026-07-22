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
        // The moment this bullet spawns (or wakes up from the Object Pool), shoot it forward!
        // In Unity 2D, transform.right is always the direction the barrel is pointing.
        rb.linearVelocity = transform.right * speed;

        // Start a timer to clean up the bullet if it flies off into space and hits nothing
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
        // Later we will add logic here to check if the thing we hit was an "Enemy".
        Debug.Log("Bullet hit: " + collision.name);
        Deactivate();
    }

    private void Deactivate()
    {
        // TEMPORARY: We are destroying the bullet so your computer doesn't crash right now.
        // Once we build the Object Pool manager, we will change this single line to: gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
