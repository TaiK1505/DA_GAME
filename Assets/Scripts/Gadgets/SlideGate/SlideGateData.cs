using UnityEngine;

[CreateAssetMenu(fileName = "New SlideGate", menuName = "Gadgets/SlideGate")]
public class SlideGateData : GadgetData
{
   [Header("Deploy Settings")]
    public GameObject slidePadPrefab; 
    public int maxActivePads = 3;       
    //public float cooldownTime = 1.5f;   

    [Header("Boost Settings")]
    public float boostForce = 40f;
    public float boostFriction = 0.5f;
}
