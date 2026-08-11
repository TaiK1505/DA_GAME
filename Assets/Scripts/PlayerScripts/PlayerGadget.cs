using UnityEngine;

public abstract class PlayerGadget : MonoBehaviour
{
    public bool isGadgetActive;
    public bool overridePlayerPhysics;
    
    public abstract void ActivateGadget();
    public abstract void DeactivateGadget();
}
