using UnityEngine;
public class MaskVision : MonoBehaviour
{
    [Header("Meter")]
    public float maxMeter        = 3f;   // seconds of vision
    public float drainRate       = 1f;   // per second while held
    public float rechargeRate    = 0.4f; // per second while released
    [Tooltip("Meter must reach this fraction before vision can activate again.")]
    public float rechargeThreshold = 0.3f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    public  float currentMeter;
    private bool  isActive   = false;
    private bool  isCharging = false;   // true while waiting for threshold

    private MazeGenerator mazeGen;

    private void Start()
    {
        currentMeter = maxMeter;
        mazeGen = FindFirstObjectByType<MazeGenerator>(); // Unity 6 API
        
        if (enableDebugLogs)
        {
            if (mazeGen == null)
                Debug.LogError("[MaskVision] MazeGenerator not found! RMB won't work.");
            else
                Debug.Log($"[MaskVision] Found MazeGenerator with {mazeGen.fakeWalls.Count} fake walls and {mazeGen.invisWalls.Count} invisible walls.");
        }
    }

    // 
    private void Update()
    {
        bool held = Input.GetMouseButton(1); // Right Mouse Button

        // Debug first press
        if (held && !isActive && enableDebugLogs)
        {
            Debug.Log($"[MaskVision] RMB pressed. isCharging={isCharging}, currentMeter={currentMeter:F2}");
        }

        if (held && !isCharging && currentMeter > 0f)
        {
            //  ACTIVATE
            if (!isActive)
            {
                ToggleVision(true);
                if (enableDebugLogs)
                    Debug.Log("[MaskVision] Vision ACTIVATED");
            }
            isActive = true;

            currentMeter -= drainRate * Time.deltaTime;
            if (currentMeter <= 0f)
            {
                currentMeter = 0f;
                ToggleVision(false);
                isActive   = false;
                isCharging = true;   // force full recharge
                
                if (enableDebugLogs)
                    Debug.Log("[MaskVision] Meter depleted. Must recharge to 30% before reuse.");
            }
        }
        else
        {
            // RELEASED / DEPLETED 
            if (isActive)
            {
                ToggleVision(false);
                isActive = false;
                
                if (enableDebugLogs)
                    Debug.Log("[MaskVision] Vision DEACTIVATED");
            }

            // Recharge
            currentMeter += rechargeRate * Time.deltaTime;
            if (currentMeter >= maxMeter)
            {
                currentMeter = maxMeter;
                isCharging   = false;
            }

            // Check if passed the recharge threshold
            if (isCharging && currentMeter >= maxMeter * rechargeThreshold)
            {
                isCharging = false;
                if (enableDebugLogs)
                    Debug.Log("[MaskVision] Recharge threshold reached. Vision available again.");
            }
        }
    }

    private void ToggleVision(bool on)
    {
        if (mazeGen == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[MaskVision] Cannot toggle vision - mazeGen is null!");
            return;
        }

        int fakeCount = 0, invisCount = 0;

        foreach (var fw in mazeGen.fakeWalls)
        {
            if (fw != null)
            {
                fw.SetRevealed(on);
                fakeCount++;
            }
        }

        foreach (var iw in mazeGen.invisWalls)
        {
            if (iw != null)
            {
                iw.SetRevealed(on);
                invisCount++;
            }
        }

        if (enableDebugLogs && on)
            Debug.Log($"[MaskVision] Toggled {fakeCount} fake walls and {invisCount} invisible walls to {(on ? "REVEALED" : "HIDDEN")}");
    }

    public float MeterRatio => currentMeter / maxMeter;
}