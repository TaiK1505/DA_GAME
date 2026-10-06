using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class StunIndicator : MonoBehaviour
{
    [Header("Visual")]
    public GameObject stunVisual;
    private EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (stunVisual != null) stunVisual.SetActive(false);
    }

    private void Update()
    {
        if (enemyAI == null || stunVisual == null) return;
        
        // It stays on for the ENTIRE stun duration perfectly.
        stunVisual.SetActive(enemyAI.IsCurrentlyStunned());
    }
}
