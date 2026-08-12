using UnityEngine;

public class SlideGateInteractable : MonoBehaviour
{
    public float boostForce = 40f; 
    public float padFrictionMultiplier;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 trapDirection = transform.up;

        // THE PLAYER
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (player != null && rb != null)
            {
                // Force the player into a high-speed slide matching the pad's direction
                player.currentState = PlayerController.State.Sliding;
                player.slideDirection = trapDirection;
                player.currentSlideSpeed = boostForce;

                player.activeBoostFriction = padFrictionMultiplier;

                rb.linearVelocity = trapDirection * boostForce;
            }
        }
        
        // THE ENEMY
        else if (collision.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
            
            if (enemyRb != null)
            {
                // launch the enemy in the pad's direction!
                enemyRb.linearVelocity = trapDirection * boostForce;
            }
            
        }
    }
}
