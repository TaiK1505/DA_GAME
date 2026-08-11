using UnityEngine;

[CreateAssetMenu(fileName = "New Grapple", menuName = "Gadgets/Grapple")]
public class GrappleData : GadgetData
{
    [Header("Grapple Physics")]
    public float grappleRange = 15f;
    public float grapplePullForce = 40f;
    public LayerMask grappleableLayer;
}
