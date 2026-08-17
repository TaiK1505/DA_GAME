using UnityEngine;

[CreateAssetMenu(fileName = "New SlideGate", menuName = "Gadgets/SlideGate")]
public class SlideGateData : GadgetData
{
   [Header("Deploy Settings")]
    public GameObject slidePadPrefab; 
    public int maxActivePads = 3;       
    //public float cooldownTime = 1.5f;   

    [Header("Slide Gate Stats")]
    public float boostForce = 40f;
    public float enemyKnockbackDuration = 0.5f;
}
