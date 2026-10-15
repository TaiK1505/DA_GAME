using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class HyperShield : MonoBehaviour
{
   [Header("Shield Tuning")]
    public float generationPerSecond = 5f;
    public float decayPerSecond = 10f;
    public float decayDelay = 5f;

    [Header("Threshold Tuning")]
    public float speedThresholdBuffer = 1f;

    private HealthComponent healthComponent;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private float currentDecayTimer = 0f;

    void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 1. Calculate the moving goalpost
        float dynamicThreshold = playerController.GetMaxNaturalSpeed() + speedThresholdBuffer;
        
        // 2. Check current physical speed
        float currentSpeed = rb.linearVelocity.magnitude;

        // 3. Apply logic
        if (currentSpeed >= dynamicThreshold)
        {
            // Pushing the limits: Generate shield
            healthComponent.AddOverhealth(generationPerSecond * Time.deltaTime);
            
            // Reset the grace period timer while moving fast
            currentDecayTimer = decayDelay;
        }
        else
        {
            // We dropped below the speed threshold. Check the grace period.
            if (currentDecayTimer > 0)
            {
                // Tick down the clock
                currentDecayTimer -= Time.deltaTime;
            }
            else
            {
                // Grace period is over: Decay shield
                healthComponent.DecayOverhealth(decayPerSecond * Time.deltaTime);
            }
        }
    }

}
