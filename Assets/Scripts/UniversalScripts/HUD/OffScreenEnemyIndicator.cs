using UnityEngine;
using System.Collections.Generic;

public class OffScreenEnemyIndicator : MonoBehaviour
{
    public static OffScreenEnemyIndicator Instance;

    [Header("Setup")]
    public GameObject uiArrowPrefab;  // The UI Image prefab we will make
    public Canvas mainCanvas;
    public float screenPadding = 50f; // Distance from the absolute edge of the monitor

    // A tiny data class to link a physical red circle to its UI arrow
    private class IndicatorPair
    {
        public Transform worldTarget;
        public RectTransform uiArrow;
    }

    private List<IndicatorPair> activeIndicators = new List<IndicatorPair>();
    private Camera mainCam;

    private void Awake()
    {
        // Singleton pattern so we can access it globally
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCam = Camera.main;
    }

    public void AddTarget(Transform target)
    {
        // When a red circle spawns, create a UI arrow for it on the Canvas
        GameObject arrowObj = Instantiate(uiArrowPrefab, mainCanvas.transform);
        IndicatorPair newPair = new IndicatorPair
        {
            worldTarget = target,
            uiArrow = arrowObj.GetComponent<RectTransform>()
        };
        activeIndicators.Add(newPair);
    }

    public void RemoveTarget(Transform target)
    {
        // When the red circle turns off, destroy its UI arrow
        for (int i = 0; i < activeIndicators.Count; i++)
        {
            if (activeIndicators[i].worldTarget == target)
            {
                Destroy(activeIndicators[i].uiArrow.gameObject);
                activeIndicators.RemoveAt(i);
                break;
            }
        }
    }

    private void Update()
    {
        foreach (var pair in activeIndicators)
        {
            if (pair.worldTarget == null) continue;

            // 1. Where is the red circle in terms of Monitor Pixels?
            Vector3 screenPos = mainCam.WorldToScreenPoint(pair.worldTarget.position);
            
            // 2. Is it outside the boundaries of the monitor?
            bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                               screenPos.y <= 0 || screenPos.y >= Screen.height;

            if (isOffScreen)
            {
                pair.uiArrow.gameObject.SetActive(true);

                // 3. Clamp the arrow to the edges of the monitor
                Vector3 clampedPos = screenPos;
                clampedPos.x = Mathf.Clamp(clampedPos.x, screenPadding, Screen.width - screenPadding);
                clampedPos.y = Mathf.Clamp(clampedPos.y, screenPadding, Screen.height - screenPadding);

                pair.uiArrow.position = clampedPos;

                // 4. Rotate the arrow to point exactly at where the red circle is off-screen
                Vector3 direction = (screenPos - clampedPos).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                pair.uiArrow.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                // If it's on screen, hide the arrow!
                pair.uiArrow.gameObject.SetActive(false);
            }
        }
    }
}
