using UnityEngine;
using System.Collections;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance;

    [Header("Room Clear Slow-Mo")]
    public float targetTimeScale = 0.2f;
    public float hangTime = 0.4f;
    public float recoverySpeed = 2.0f;

    // ---> NEW: Moved from HealthComponent <---
    [Header("Player Damage Feedback")]
    [Tooltip("How long the screen shakes when hit")]
    public float damageShakeDuration = 0.1f; 
    [Tooltip("How violent the shake is (radius of the shake circle)")]
    public float damageShakeMagnitude = 0.15f; 

    private Coroutine slowMoCoroutine;
    private float defaultFixedDeltaTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        defaultFixedDeltaTime = Time.fixedDeltaTime; 
    }

    // ---> NEW: The Master Method for getting hit <---
    public void TriggerPlayerDamageFeel()
    {
        // 1. Camera Shake
        if (CameraController.Instance != null)
        {
            CameraController.Instance.TriggerShake(damageShakeDuration, damageShakeMagnitude); 
        }

        // 2. Future: Add Controller Rumble here!
        // 3. Future: Add Audio Ducking here!
        // 4. Future: Add Hit-Stop (Time Freeze) here!
    }

    public void TriggerRoomClearSlowMo()
    {
        if (slowMoCoroutine != null) StopCoroutine(slowMoCoroutine);
        slowMoCoroutine = StartCoroutine(RoomClearRoutine());
    }

    private IEnumerator RoomClearRoutine()
    {
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale; 

        yield return new WaitForSecondsRealtime(hangTime);

        float currentScale = Time.timeScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * recoverySpeed; 
            
            Time.timeScale = Mathf.Lerp(currentScale, 1f, t);
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}