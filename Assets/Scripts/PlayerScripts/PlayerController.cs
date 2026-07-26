using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    public enum State { Idle, Running, Sliding, Dashing }
    public State currentState;
    
    [Header("Movement Stats")]
    public float moveSpeed = 8f;

    [Header("Dash Stats")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f; 
    public float dashCooldown = 1f;

    [Header("Slide Stats")]
    public float slideMultiplier = 2.5f; // Scales initial burst off your current move speed
    public float dashSlideDampener = 0.7f;
    public float slideFriction = 40f;    // How fast you lose speed during the slide
    public float minSlideSpeed = 5f;     // The speed at which the slide cancels
    public float slideCooldown = 0.5f;
    public float slideSteeringFactor = 3f;  // How much control WASD has during a slide

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 dashDirection;
    private float dashTimeLeft;
    private float lastDashTime = -100f;

    private Vector2 slideDirection;
    private float currentSlideSpeed;
    private float lastSlideTime = -100f;

    private PlayerControls controls;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 3. Initialize the controls
        controls = new PlayerControls();

        // 4. The "Tripwire": When the Dash button is performed, fire the AttemptDash method!
        controls.Player.Dash.performed += ctx => AttemptDash();
        controls.Player.Slide.performed += ctx => AttemptSlide();
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

        switch (currentState)
        {
            case State.Idle:
                

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
                if (movementInput != Vector2.zero)
                {
                    slideDirection = Vector2.Lerp(slideDirection, movementInput.normalized, slideSteeringFactor * Time.deltaTime).normalized;
                }

                // FRICTION LOGIC
                currentSlideSpeed -= slideFriction * Time.deltaTime;
                
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
                    currentState = State.Idle;
                }
                break;
        }

    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                break;

            case State.Running:
                rb.linearVelocity = movementInput.normalized * moveSpeed;
                break;
                
            case State.Dashing:
                rb.linearVelocity = dashDirection * dashSpeed;
                break;
            case State.Sliding:
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
}
