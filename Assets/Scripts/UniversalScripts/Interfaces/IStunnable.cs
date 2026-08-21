using UnityEngine;

public interface IStunnable
{
    bool IsCurrentlyStunned();

    void Stun(float duration);
} 