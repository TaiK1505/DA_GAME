using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleGadget : PlayerGadget
{
    
    
    [Header("Stats")]
    public GrappleData myStats;
    
    [Header("Grapple Limitations")]
    public float maxHoldTime = 1.5f; 
    public float maxGrappleSpeed = 35f;

    private float currentHoldTime;
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private Vector2 grapplePoint;
    private bool isGrappling;
    private float currentMaxRopeLength;
    
    private void Awake()
    {
        // Grab the components off the Player
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        
        // Hide the rope when the game starts
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
    
    public override void ActivateGadget()
    {
        // Find the direction to the mouse
        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
        
        Vector2 grappleDir = (mousePos - (Vector2)transform.position).normalized;

        // Shoot an Raycast to find a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, grappleDir, myStats.grappleRange, myStats.grappleableLayer);

        if (hit.collider != null)
        {
            // Anchor in and turn on the rope.
            grapplePoint = hit.point;
            isGadgetActive = true;
            overridePlayerPhysics = true;

            currentHoldTime = maxHoldTime;

            currentMaxRopeLength = Vector2.Distance(transform.position, grapplePoint);  
            
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            } 
            
        }
    }

    public override void DeactivateGadget()
    {
        // Player let go of the button. Cut the rope!
        isGadgetActive = false;
        overridePlayerPhysics = false;
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    private void Update()
    {
        // Visuals go in standard Update so the rope doesn't stutter
        if (isGadgetActive)
        {
            lineRenderer.SetPosition(0, transform.position); // Point A: Player
            lineRenderer.SetPosition(1, grapplePoint);       // Point B: The Wall
        }

        currentHoldTime -= Time.deltaTime;
        if (currentHoldTime <= 0)
        {   
            DeactivateGadget(); 
        }
    }

    private void FixedUpdate()
    {
        if (isGadgetActive) 
        {
            // Get our math vectors
            Vector2 toAnchor = grapplePoint - (Vector2)transform.position;
            float currentDistance = toAnchor.magnitude;
            Vector2 pullDir = toAnchor.normalized;

            // apply the inward thruster
            rb.AddForce(pullDir * myStats.grapplePullForce, ForceMode2D.Force);

            // 2. THE WINCH: If the player gets pulled closer, shrink the maximum rope length!
            // the rope can get shorter, but never longer.
            if (currentDistance < currentMaxRopeLength)
            {
                currentMaxRopeLength = currentDistance;
            }

            // 3. THE RIGID TETHER: Prevent the Bungee Cord!
            if (currentDistance >= currentMaxRopeLength)
            {
                // Calculate which way is "Outward" (away from the pillar)
                Vector2 outwardDir = -pullDir;
                
                // Check if our current physics velocity is pushing us outward
                float outwardSpeed = Vector2.Dot(rb.linearVelocity, outwardDir);

                // If we are flying outward, cancel ONLY the outward speed!
                if (outwardSpeed > 0)
                {
                    // This strips away the bungee stretch
                    rb.linearVelocity -= outwardDir * outwardSpeed;
                }
                
                // Hard-clamp the position just in case the physics engine stutters
                transform.position = grapplePoint + (outwardDir * currentMaxRopeLength);
            }

            if (rb.linearVelocity.magnitude > maxGrappleSpeed)
        {
            // Strip the speed back down to the maximum allowed limit
            rb.linearVelocity = rb.linearVelocity.normalized * maxGrappleSpeed;
        }
        }
    }

}
