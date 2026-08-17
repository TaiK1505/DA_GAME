using UnityEngine;

public class SlideGateInteractable : MonoBehaviour
{
    [HideInInspector] public float boostForce; 
    [HideInInspector] public float knockbackDuration;
    

    
    public void InitializePad(float durationFromSO)
    {
        knockbackDuration = durationFromSO;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 trapDirection = transform.up;
        Debug.Log("THE PAD WAS TOUCHED BY: " + collision.gameObject.name);

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


                rb.linearVelocity = trapDirection * boostForce;
            }
        }
        
        // THE ENEMY
        else if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemyScript = collision.GetComponent<EnemyAI>();    
                    
            if (enemyScript != null)
            {
                // launch the enemy in the pad's direction!
                // pause their A* brain for 0.4 seconds so they physically fly across the room.
                enemyScript.ApplyKnockback(trapDirection * boostForce, knockbackDuration);
            }
        }
    }
}
