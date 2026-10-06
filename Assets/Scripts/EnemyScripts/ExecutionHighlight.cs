using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyAI))]
public class ExecutionHighlight : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer enemyRenderer;
    
    [Header("Outline vs Color Swap")]
    [Tooltip("When you add real art, check this box to switch back to the White Outline shader!")]
    public bool useShaderOutline = false;
    public Color executionPink = new Color(1f, 0.1f, 0.8f); 
    
    private EnemyAI enemyAI;
    private Transform playerTransform;
    private MeleeWeaponController playerWeapon;
    
    private MaterialPropertyBlock propBlock;
    private static readonly int OutlineAlpha = Shader.PropertyToID("_EnableOutline");
    private Color originalColor;
    
    // ---> NEW: Tracks if WE changed the color, so we don't fight the other scripts! <---
    private bool isColorOverridden = false;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        propBlock = new MaterialPropertyBlock();
        
        if (enemyRenderer != null) 
        {
            originalColor = enemyRenderer.color;
        }

        StartCoroutine(DelayedMaterialSetup());
    }

    private IEnumerator DelayedMaterialSetup()
    {
        yield return null;
        SetOutline(0f);
    }

    // ---> NEW: LateUpdate runs AFTER the enemy scripts, guaranteeing it wins the color fight <---
    private void LateUpdate() 
    {
        if (enemyAI == null || enemyRenderer == null) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (playerWeapon == null && playerTransform != null)
        {
            playerWeapon = playerTransform.GetComponentInChildren<MeleeWeaponController>();
        }

        // 1. If NOT stunned, hands off the color! (Just reset it once if we made it pink)
        if (!enemyAI.IsCurrentlyStunned())
        {
            if (isColorOverridden)
            {
                enemyRenderer.color = originalColor;
                isColorOverridden = false;
            }
            SetOutline(0f);
            return;
        }

        // 2. If Stunned, calculate distance
        bool inExecutionRange = false;
        if (playerTransform != null && playerWeapon != null && playerWeapon.weaponData != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= playerWeapon.weaponData.executionLungeDistance)
            {
                inExecutionRange = true;
            }
        }

        // 3. Apply visuals safely
        if (useShaderOutline)
        {
            // If the checkbox is checked, ensure we aren't painting them pink
            if (isColorOverridden) 
            {
                enemyRenderer.color = originalColor;
                isColorOverridden = false;
            }
            SetOutline(inExecutionRange ? 1f : 0f);
        }
        else
        {
            // The Color Swap Logic
            if (inExecutionRange)
            {
                enemyRenderer.color = executionPink;
                isColorOverridden = true;
            }
            else
            {
                // We stepped out of range, revert the pink
                if (isColorOverridden)
                {
                    enemyRenderer.color = originalColor;
                    isColorOverridden = false;
                }
            }
            SetOutline(0f);
        }
    }

    private void SetOutline(float value)
    {
        if (enemyRenderer != null && propBlock != null)
        {
            enemyRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(OutlineAlpha, value);
            enemyRenderer.SetPropertyBlock(propBlock);
        }
    }
}
