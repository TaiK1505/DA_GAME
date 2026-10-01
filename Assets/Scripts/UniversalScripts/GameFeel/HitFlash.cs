using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Material flashMaterial;
    public float flashDuration = 0.1f;

    private Material originalMaterial;
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalMaterial = spriteRenderer.material;
    }

    public void Flash()
    {
        if (spriteRenderer == null || flashMaterial == null) return;
        
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Swap to the pure white silhouette
        spriteRenderer.material = flashMaterial;
        
        // Wait for a split second
        yield return new WaitForSeconds(flashDuration);
        
        // Swap back to the normal sprite
        spriteRenderer.material = originalMaterial;
    }

    private void OnDisable()
    {
        // Safety check: if the enemy dies or gets pooled during a flash, reset their material
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

}
