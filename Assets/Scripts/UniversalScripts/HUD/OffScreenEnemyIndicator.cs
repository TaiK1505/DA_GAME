using UnityEngine;
using System.Collections.Generic;

public class OffScreenEnemyIndicator : MonoBehaviour
{
    public static OffScreenEnemyIndicator Instance;

    [Header("Setup")]
    public GameObject uiArrowPrefab;  // The UI Image prefab we will make
    public Canvas mainCanvas;
    public float screenPadding = 50f; // Distance from the absolute edge of the monitor

    [Header("Fan Out Settings")]
    public float fanOutSpacing = 60f; // Minimum pixels allowed between arrows

    private class IndicatorPair
    {
        public Transform worldTarget;
        public RectTransform uiArrow;
        public Vector3 currentScreenPos; 
    }

    private List<IndicatorPair> activeIndicators = new List<IndicatorPair>();
    private Camera mainCam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCam = Camera.main;
    }

    public void AddTarget(Transform target)
    {
        GameObject arrowObj = Instantiate(uiArrowPrefab, mainCanvas.transform);
        IndicatorPair newPair = new IndicatorPair
        {
            worldTarget = target,
            uiArrow = arrowObj.GetComponent<RectTransform>(),
            currentScreenPos = Vector3.zero
        };
        activeIndicators.Add(newPair);
    }

    public void RemoveTarget(Transform target)
    {
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
        if (mainCam == null) return;

        // Step 1: Clamp everyone to the edge (They will overlap here)
        for (int i = 0; i < activeIndicators.Count; i++)
        {
            var pair = activeIndicators[i];
            if (pair.worldTarget == null) continue;

            Vector3 trueScreenPos = mainCam.WorldToScreenPoint(pair.worldTarget.position);
            
            if (trueScreenPos.z < 0) trueScreenPos *= -1; 

            bool isOffScreen = trueScreenPos.x <= 0 || trueScreenPos.x >= Screen.width ||
                               trueScreenPos.y <= 0 || trueScreenPos.y >= Screen.height;

            if (isOffScreen)
            {
                pair.uiArrow.gameObject.SetActive(true);
                pair.currentScreenPos = ClampToScreenEdge(trueScreenPos);
            }
            else
            {
                pair.uiArrow.gameObject.SetActive(false);
            }
        }

        // Step 2: Instant Overlap Resolution! 
        // We run this 3 times in a row instantly to force chained arrows to cascade cleanly
        for (int iteration = 0; iteration < 3; iteration++)
        {
            for (int i = 0; i < activeIndicators.Count; i++)
            {
                if (!activeIndicators[i].uiArrow.gameObject.activeSelf) continue;

                for (int j = i + 1; j < activeIndicators.Count; j++)
                {
                    if (!activeIndicators[j].uiArrow.gameObject.activeSelf) continue;

                    Vector3 posA = activeIndicators[i].currentScreenPos;
                    Vector3 posB = activeIndicators[j].currentScreenPos;

                    float distance = Vector3.Distance(posA, posB);
                    
                    if (distance < fanOutSpacing)
                    {
                        Vector3 pushDir = (posA - posB).normalized;

                        // THE FIX: If they share the exact same spawn point pixel, give them a push direction manually!
                        if (pushDir == Vector3.zero)
                        {
                            // If they are on the Left or Right wall, slide them vertically
                            if (posA.x <= screenPadding + 5f || posA.x >= Screen.width - screenPadding - 5f)
                            {
                                pushDir = Vector3.up;
                            }
                            // If they are on the Top or Bottom ceiling/floor, slide them horizontally
                            else
                            {
                                pushDir = Vector3.right;
                            }
                        }

                        // Push them apart instantly by the exact amount they are overlapping
                        float pushAmount = (fanOutSpacing - distance) / 2f;
                        
                        activeIndicators[i].currentScreenPos += pushDir * pushAmount;
                        activeIndicators[j].currentScreenPos -= pushDir * pushAmount;

                        // Re-clamp so the push doesn't knock them off the monitor!
                        activeIndicators[i].currentScreenPos = ClampToScreenEdge(activeIndicators[i].currentScreenPos);
                        activeIndicators[j].currentScreenPos = ClampToScreenEdge(activeIndicators[j].currentScreenPos);
                    }
                }
            }
        }

        // Step 3: Apply the physical positions and aim them
        for (int i = 0; i < activeIndicators.Count; i++)
        {
            var pair = activeIndicators[i];
            if (!pair.uiArrow.gameObject.activeSelf || pair.worldTarget == null) continue;

            pair.uiArrow.position = pair.currentScreenPos;

            Vector3 trueScreenPos = mainCam.WorldToScreenPoint(pair.worldTarget.position);
            if (trueScreenPos.z < 0) trueScreenPos *= -1; 

            // Aim from the fanned-out spot directly to the enemy
            Vector3 direction = (trueScreenPos - pair.currentScreenPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            pair.uiArrow.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private Vector3 ClampToScreenEdge(Vector3 rawPos)
    {
        rawPos.x = Mathf.Clamp(rawPos.x, screenPadding, Screen.width - screenPadding);
        rawPos.y = Mathf.Clamp(rawPos.y, screenPadding, Screen.height - screenPadding);
        return rawPos;
    }
}
