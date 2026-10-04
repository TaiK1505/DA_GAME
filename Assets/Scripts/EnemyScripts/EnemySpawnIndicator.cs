using UnityEngine;
using System.Collections;

public class EnemySpawnIndicator : MonoBehaviour
{
    
    public void Initialize(GameObject enemyToSpawn)
    {
        // will add cool visual color-changing code here later
    }

    private void OnEnable()
    {
        // The moment the Object Pool turns this red circle ON, tell the Canvas to track it
        if (OffScreenEnemyIndicator.Instance != null)
        {
            OffScreenEnemyIndicator.Instance.AddTarget(this.transform);
        }
    }

    private void OnDisable()
    {
        // The moment the Object Pool turns this red circle OFF, tell the Canvas to delete the arrow
        if (OffScreenEnemyIndicator.Instance != null)
        {
            OffScreenEnemyIndicator.Instance.RemoveTarget(this.transform);
        }
    }
}
