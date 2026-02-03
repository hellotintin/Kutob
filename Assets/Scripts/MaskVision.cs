using UnityEngine;
using UnityEngine.InputSystem;  

public class MaskVision : MonoBehaviour
{
    [Header("Meter")]
    public float maxMeter = 3f;
    public float drainRate = 1f;
    public float rechargeRate = 0.4f;
    public float rechargeThreshold = 0.3f;

    public float currentMeter;
    private bool isActive = false;
    private bool isCharging = false;
    private bool visionHeld = false;  // Track if RMB is held

    private MazeGenerator mazeGen;

    private void Start()
    {
        currentMeter = maxMeter;
        mazeGen = FindFirstObjectByType<MazeGenerator>();
    }

    // Called by Input System when RMB is pressed/released
    public void OnMaskVision(InputValue value)
    {
        visionHeld = value.isPressed;
    }

    private void Update()
    {
        if (visionHeld && !isCharging && currentMeter > 0f)
        {
            //  ACTIVATE 
            if (!isActive) ToggleVision(true);
            isActive = true;

            currentMeter -= drainRate * Time.deltaTime;
            if (currentMeter <= 0f)
            {
                currentMeter = 0f;
                ToggleVision(false);
                isActive = false;
                isCharging = true;
            }
        }
        else
        {
            if (isActive)
            {
                ToggleVision(false);
                isActive = false;
            }

            // Recharge
            currentMeter += rechargeRate * Time.deltaTime;
            if (currentMeter >= maxMeter)
            {
                currentMeter = maxMeter;
                isCharging = false;
            }

            if (isCharging && currentMeter >= maxMeter * rechargeThreshold)
                isCharging = false;
        }
    }

    private void ToggleVision(bool on)
    {
        if (mazeGen == null) return;

        foreach (var fw in mazeGen.fakeWalls)
            fw.SetRevealed(on);

        foreach (var iw in mazeGen.invisWalls)
            iw.SetRevealed(on);
    }

    public float MeterRatio => currentMeter / maxMeter;
}