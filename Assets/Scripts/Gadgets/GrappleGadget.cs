using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleGadget : PlayerGadget
{
    
    
    [Header("Stats")]
    public GrappleData myStats; // Drag your ScriptableObject here in the Inspector!

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private Vector2 grapplePoint;
    private bool isGrappling;
    
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
    }

    private void FixedUpdate()
    {
        // Physics go in FixedUpdate
        if (isGadgetActive)
        {
            // Calculate the direction pulling us toward the anchor
            Vector2 pullDir = (grapplePoint - (Vector2)transform.position).normalized;

            // Apply the force! 
            // This blends with your slide velocity to create the circular Pathfinder swing.
            rb.AddForce(pullDir * myStats.grapplePullForce, ForceMode2D.Force);
        }
    }

}
