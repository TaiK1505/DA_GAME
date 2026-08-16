using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    public enum State { Idle, Running, Sliding, Dashing }
    public State currentState;
    
    [Header("Movement Stats")]
    private float moveSpeed => myStats.GetCurrentSpeed();   

    [Header("Dash Stats")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f; 
    public float dashCooldown = 1f;

    [Header("Slide Stats")]
    public float slideMultiplier = 2.5f; // Scales initial burst off your current move speed
    public float dashSlideDampener;
    public float slideFriction = 40f;    // How fast you lose speed during the slide
    public float minSlideSpeed = 5f;     // The speed at which the slide cancels
    public float slideCooldown = 0.5f;
    public float slideSteeringFactor = 3f; 
    
    [Header("Ramming Stats")]
    public float ramForceMultiplier = 1.2f;

    [Header("Gadget Stats")]
    public float grappleStrafeForce = 15f;
    public float activeBoostFriction = 0.5f;

    [Header("Interaction")]
    public float interactRange = 1.5f;
    public LayerMask interactableLayer;

    public PlayerGadget currentActiveGadget;
    
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 dashDirection;
    private float dashTimeLeft;
    private float lastDashTime = -100f;

    public Vector2 slideDirection;
    public float currentSlideSpeed;
    
    private float lastSlideTime = -100f;

    private PlayerStats myStats;

    private PlayerControls controls;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        myStats = GetComponent<PlayerStats>();

        // Initialize the controls
        controls = new PlayerControls();

        // 4. The "Tripwire": When the Dash button is performed, fire the AttemptDash method!
        controls.Player.Dash.performed += ctx => AttemptDash();
        controls.Player.Slide.performed += ctx => AttemptSlide();
        
        controls.Player.Gadget.started += ctx => currentActiveGadget?.ActivateGadget();
        controls.Player.Gadget.canceled += ctx => currentActiveGadget?.DeactivateGadget();

        controls.Player.Interact.performed += ctx => TryInteract();
    }
    
        void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        currentState = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        movementInput = controls.Player.Move.ReadValue<Vector2>();

        bool isGadgetPulling = currentActiveGadget != null && currentActiveGadget.overridePlayerPhysics;

        switch (currentState)
        {
            case State.Idle:
                if (movementInput.sqrMagnitude > 0)
                    {
                        currentState = State.Running;
                    }
                    break;
                

            case State.Running:
                if (movementInput.sqrMagnitude > 0) 
                {
                    currentState = State.Running;
                } 
                else
                {
                    currentState = State.Idle;
                }
                break;

            case State.Sliding:
                float maxNaturalSpeed = GetMaxNaturalSpeed();

                Debug.Log($"SLIDING STATE | Current Speed: {currentSlideSpeed:F1} | Max Natural Speed: {maxNaturalSpeed:F1} | Is Boosted: {currentSlideSpeed > maxNaturalSpeed + 1f}");
                
                
                if (movementInput != Vector2.zero)
                {
                    float currentSteering = slideSteeringFactor;
                    
                    if (currentSlideSpeed > maxNaturalSpeed + 1f)
                    {
                        currentSteering *= 2.5f; // Tweak this for sharper/looser boosted turns
                    }
                    
                    slideDirection = Vector2.Lerp(slideDirection, movementInput.normalized, slideSteeringFactor * Time.deltaTime).normalized;
                }

                //rb.linearVelocity = slideDirection * currentSlideSpeed;
                
                if (!isGadgetPulling)
                {
                    // Dynamic Friction: Bleed off massive blast speeds faster so we don't slide forever
                    if (currentSlideSpeed > maxNaturalSpeed + 1f) 
                    {
                        // Add myStats.GetCurrentFrictionMultiplier() to the math!
                        currentSlideSpeed -= (slideFriction * myStats.GetCurrentFrictionMultiplier() * activeBoostFriction) * Time.deltaTime; 
                    }
                    else 
                    {
                        currentSlideSpeed -= (slideFriction * myStats.GetCurrentFrictionMultiplier()) * Time.deltaTime;
                    }
                }
                
                
                // EXIT LOGIC
                if (currentSlideSpeed <= minSlideSpeed)
                {
                    currentState = State.Idle;
                }
                break;

            case State.Dashing:
                dashTimeLeft -= Time.deltaTime;
                if (dashTimeLeft <= 0)
                {
                    // DASH IS OVER! Where are we going, and how fast?
                    if (movementInput.sqrMagnitude > 0)
                    {
                        currentState = State.Running;
                        rb.linearVelocity = movementInput.normalized * moveSpeed; // SNAPPY RUN EXIT!
                    }
                    else
                    {
                        currentState = State.Idle;
                        rb.linearVelocity = Vector2.zero; // SNAPPY IDLE EXIT!
                    }
                }
                break;
        }

    }

    void FixedUpdate()
    {
        bool isGadgetPulling = currentActiveGadget != null && currentActiveGadget.overridePlayerPhysics;

        if (isGadgetPulling && movementInput != Vector2.zero)
        {
            rb.AddForce(movementInput.normalized * grappleStrafeForce, ForceMode2D.Force);
        }
        
        switch (currentState)
        {
            case State.Idle:
                if (isGadgetPulling) break; // Hands off! Let the grapple pull us.
                
                // MOMENTUM BLEED: slide to a stop smoothly
                if (rb.linearVelocity.magnitude > moveSpeed + 0.5f) 
                {
                    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 3f);
                }
                 else 
                {
                    rb.linearVelocity = Vector2.zero;
                }
                break;

            case State.Running:
                if (isGadgetPulling) break; // Hands off! Let the grapple pull us.
                
                // MOMENTUM BLEED: If we are flying faster than our run speed, blend into our normal run smoothly
                if (rb.linearVelocity.magnitude > moveSpeed) 
                {
                    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, movementInput.normalized * moveSpeed, Time.fixedDeltaTime * 3f);
                } 
                else 
                {
                    rb.linearVelocity = movementInput.normalized * moveSpeed;
                }
                break;
                
            case State.Dashing:
                if (isGadgetPulling) break; // Hands off!
                
                rb.linearVelocity = dashDirection * dashSpeed;
                break;

            case State.Sliding:
                if (isGadgetPulling) 
                {
                    // While the grapple is throwing us around, sync the slide variables.
                    // when player let go, the slide seamlessly takes over at the new angle and speed!
                    currentSlideSpeed = rb.linearVelocity.magnitude;
                    slideDirection = rb.linearVelocity.normalized;
                    break; // Hands off!
                }

                rb.linearVelocity = slideDirection * currentSlideSpeed;
                break;    
        } 
    }

    private void AttemptDash()
    {
        
        if ((currentState == State.Idle || currentState == State.Running) && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        currentState = State.Dashing;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        if (movementInput != Vector2.zero)
        {
            dashDirection = movementInput.normalized;
        }
        else
        {
            dashDirection = new Vector2(1, 0); 
        }
    }

    private void AttemptSlide()
    {
        if ((currentState == State.Running || currentState == State.Dashing) && Time.time >= lastSlideTime + slideCooldown)
        {
            StartSlide();
        }
    }

     private void StartSlide()
    {
        // 2. MOMENTUM CHECK
        float momentumBase = (currentState == State.Dashing) ? (dashSpeed * dashSlideDampener) : moveSpeed;
        // 3. DIRECTION CHECK
        Vector2 startingDir = movementInput.normalized;
        if (startingDir == Vector2.zero && currentState == State.Dashing)
        {
            startingDir = dashDirection;
        }

        currentState = State.Sliding;
        currentSlideSpeed = momentumBase * slideMultiplier; 
        lastSlideTime = Time.time;
        slideDirection = startingDir; 
    }

    public void OnGadgetButton()
    {
        if (currentActiveGadget != null)
        {
            // The PlayerController just presses the big red "GO" button.
            // It doesn't care if this triggers a grapple or an explosion.
            currentActiveGadget.ActivateGadget(); 
        }
    }

    public void TryInteract()
    {
        Debug.Log("1. INTERACT BUTTON PRESSED!");

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayer);
        Debug.Log("2. Found " + hitColliders.Length + " objects on the Interactable Layer in range.");

        foreach (Collider2D hit in hitColliders)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log("3. Found the Pedestal Script! Trying to equip...");
                interactable.Interact(this.gameObject);
                return; 
            }
        }
    }

    public void EquipGadget(GadgetData newGadgetData, string newScriptName)
    {
        // 1. Destroy the old gadget script if you are already holding one
        if (currentActiveGadget != null)
        {
            Destroy(currentActiveGadget);
        }

        // 2. Find the new script by its name
        System.Type scriptType = System.Type.GetType(newScriptName);
        
        if (scriptType != null && scriptType.IsSubclassOf(typeof(PlayerGadget)))
        {
            // 3. Attach the new script to the player
            currentActiveGadget = (PlayerGadget)gameObject.AddComponent(scriptType);

            // 4. Shove the Stat Card data into the newly attached script!
            currentActiveGadget.InitializeGadget(newGadgetData); 
        }
        else
        {
            Debug.LogError("Could not find a Gadget Script named: " + newScriptName + ". Check your spelling!");
        }
    }
    
    private float GetMaxNaturalSpeed()
    {
        return (dashSpeed * slideMultiplier) * dashSlideDampener;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Check the impact speed using relative velocity
        float impactSpeed = collision.relativeVelocity.magnitude;
        float maxNaturalSpeed = GetMaxNaturalSpeed();

        if (impactSpeed > maxNaturalSpeed + 1f)
        {
            // 2. Look for the EnemyAI script
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            
            if (enemy != null)
            {
                Debug.Log($"RAMMED ENEMY! Impact Speed: {impactSpeed:F1}");

                // 3. Calculate direction away from you
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                
                
                // The faster player are moving when you hit them the harder they fly
                float dynamicKnockbackForce = impactSpeed * ramForceMultiplier;

                //Send the scaled force to the enemy
                enemy.ApplyKnockback(knockbackDir * dynamicKnockbackForce, 0.5f); 
            }
        }
    }
}
