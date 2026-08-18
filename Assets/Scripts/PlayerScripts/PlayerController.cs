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

    [Header("Wall Boost Mechanics")]
    public float wallBoostThreshold = 12f;     // minimum speed required to trigger a boost
    public float wallBoostMultiplier = 1.3f;   // Multiplies incoming speed by 1.3x (30% increase)
    public float maxWallBoostSpeed = 40f;      // The hard cap so players don't break the physics engine
    public float wallBoostCoyoteTime = 0.5f;

    [Header("Wall Boost Limits")]
    public int maxWallBoosts = 3;            // Total number of charges
    public float boostRechargeTime = 2f;     // Time it takes to recharge ONE charge

    public int currentWallBoosts;            
    public float boostRechargeTimer;
    private int currentBoostCount = 0;
    private float boostCooldownTimer = 0f;
    private float currentComboTimer = 0f;
    private Vector2 lastFrameVelocity;
    private bool canWallBoost;
    private float wallBoostTimer;
    private Vector2 currentWallNormal;
    private Vector2 capturedImpactVelocity;

    [Header("Input Buffering")]
    public float boostBufferTime = 0.15f; // How early they can press the button before hitting the wall
    private float lastBoostInputTime = -100f; // Memory of the exact time they pressed the button


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
        controls.Player.Dash.performed += ctx => HandleDashInput();
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
        currentWallBoosts = maxWallBoosts;
    }

    // Update is called once per frame
    void Update()
    {
        if (canWallBoost)
        {
            wallBoostTimer -= Time.deltaTime;
            
            if (wallBoostTimer <= 0)
            {
                canWallBoost = false;
                // Debug.Log("Wall Boost Window CLOSED."); 
            }
        }
        if (currentWallBoosts < maxWallBoosts)
        {
            // Tick the clock UP
            boostRechargeTimer += Time.deltaTime;
            
            // Did we hit the 2-second mark?
            if (boostRechargeTimer >= boostRechargeTime)
            {
                currentWallBoosts++;           // Give them 1 charge back!
                boostRechargeTimer = 0f;       // Reset the clock to start building the NEXT charge
            }
        }
        else
        {
            // Keep the clock completely zeroed out when the tank is full
            boostRechargeTimer = 0f; 
        }
        
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

        if (rb.linearVelocity.magnitude > 1f) 
        {
            lastFrameVelocity = rb.linearVelocity;
        }

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
        // 1. Get our RAW speed based on our state, completely ignoring the enemy!
        float myRawSpeed = 0f;
        if (currentState == State.Sliding) myRawSpeed = currentSlideSpeed;
        else if (currentState == State.Dashing) myRawSpeed = dashSpeed;
        else myRawSpeed = rb.linearVelocity.magnitude;

        // 2. Are we going fast enough to ram?
        if (myRawSpeed > GetMaxNaturalSpeed() + 1f)
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            
            if (enemy != null)
            {
                Debug.Log($"RAMMED ENEMY! Raw Speed: {myRawSpeed:F1}");

                // 3. Calculate direction away from us
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                
                // 4. Calculate exact, predictable force
                float dynamicKnockbackForce = myRawSpeed * ramForceMultiplier;

                // 5. Blast them!
                enemy.ApplyKnockback(knockbackDir * dynamicKnockbackForce, 0.5f); 

                // 6. THE MOMENTUM TAX: Player loses 15% of their speed on impact!
                // This stops the "grinding" against ranged enemies and makes the hit feel heavy.
                if (currentState == State.Sliding)
                {
                    currentSlideSpeed *= 0.85f; 
                }
            }
        }

        if (collision.gameObject.CompareTag("Wall")) 
        {
            if (boostCooldownTimer > 0) return;
            
            if (lastFrameVelocity.magnitude >= wallBoostThreshold)
            {
                canWallBoost = true;
                wallBoostTimer = wallBoostCoyoteTime;
                currentWallNormal = collision.contacts[0].normal;

                capturedImpactVelocity = lastFrameVelocity; 
                
                // Did they press the button just before hitting the wall?
                if (Time.time - lastBoostInputTime <= boostBufferTime && currentWallBoosts > 0)
                {
                    TryWallBoost(); // Launch them IMMEDIATELY!
                    
                    // Consume the input so it doesn't double-fire
                    lastBoostInputTime = -100f; 
                }
                else
                {
                    // If they didn't pre-press it, just open the Coyote Time window like normal
                    Debug.Log("Wall Boost Window OPEN! Captured speed: " + capturedImpactVelocity.magnitude);
                }
            }

            ScrubWallMomentum(collision, true);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall")) 
        {
            // Constantly scrub velocity pushing into the wall, but pass 'false' 
            // so we don't aggressively kill your speed while you are just gliding along it!
            ScrubWallMomentum(collision, false); 
        }
    }

    public void TryWallBoost() 
    {
        if (!canWallBoost) return; 
        if (currentWallBoosts <= 0) return;

        if (boostCooldownTimer > 0) return;

        // 1. Calculate using the CAPTURED velocity snapshot!
        Vector2 boostDirection = Vector2.Reflect(capturedImpactVelocity.normalized, currentWallNormal);

        // 2. Multiply their CAPTURED incoming speed!
        float incomingSpeed = capturedImpactVelocity.magnitude;
        float outgoingSpeed = incomingSpeed * wallBoostMultiplier;

        // 3. Cap the speed
        outgoingSpeed = Mathf.Min(outgoingSpeed, maxWallBoostSpeed);

        // 4. Hijack the Sliding State to physically launch the player!
        currentState = State.Sliding;
        currentSlideSpeed = outgoingSpeed;
        slideDirection = boostDirection;

        canWallBoost = false;

        currentWallBoosts--;

        Debug.Log("KICKSTART! Hijacked Slide Speed: " + outgoingSpeed);
    }
    
    private void HandleDashInput()
    {
        // 1. STAMP THE TIME! We remember exactly when they pressed the button.
        lastBoostInputTime = Time.time; 

        // PRIORITY 1: Are we against a wall with high momentum?
        if (canWallBoost && currentWallBoosts > 0)
        {
            TryWallBoost();
        }
        // PRIORITY 2: If no wall boost is available, do a normal dash.
        else
        {
            AttemptDash();
        }
    }

    private void ScrubWallMomentum(Collision2D collision, bool isInitialImpact)
    {
        if (currentState != State.Sliding) return;

        Vector2 wallNormal = collision.contacts[0].normal;

        // A Vector Dot Product checks if two directions are facing opposite ways. 
        // If it's less than 0, it means our slide is actively pushing INTO the wall!
        if (Vector2.Dot(slideDirection, wallNormal) < 0)
        {
            // Calculate a new direction that runs perfectly PARALLEL to the wall (Wall Gliding!)
            Vector2 slideAlongWall = slideDirection - (Vector2.Dot(slideDirection, wallNormal) * wallNormal);

            // On the very first frame we hit the wall, we apply a "crash tax"
            if (isInitialImpact)
            {
                // If we hit head-on, magnitude is 0 (we stop dead). 
                // If it's a glancing blow, we keep almost all our speed!
                currentSlideSpeed *= slideAlongWall.magnitude;
            }

            // Re-direct the player
            if (slideAlongWall.magnitude > 0.01f)
            {
                slideDirection = slideAlongWall.normalized;
            }
            else
            {
                // We are stuffed perfectly in a corner. Kill the phantom momentum!
                currentSlideSpeed = 0f;
            }
        }
    }
}