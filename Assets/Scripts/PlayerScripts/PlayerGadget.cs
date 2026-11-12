using UnityEngine;

public abstract class PlayerGadget : MonoBehaviour
{
    public bool isGadgetActive;
    public bool overridePlayerPhysics;
    
    public abstract void ActivateGadget();
    public abstract void DeactivateGadget();

    public virtual void InitializeGadget(GadgetData data)
    {
        // gadgets will override this
    }

    protected float GetEffectiveCooldown(float baseCooldown)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        float cdr = stats != null ? stats.GetCurrentGadgetCooldownReduction() : 0f;
        return baseCooldown * (1f - cdr);
    }
}
