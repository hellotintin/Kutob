using UnityEngine;

/// <summary>
/// Mask Vision system.
/// Hold Right Mouse Button to activate.
/// While active: fake walls go semi-transparent red,
///               invisible walls flash visible in blue.
/// A meter drains while held; recharges slowly when released.
/// When meter hits 0 the vision cuts out and must fully recharge
/// before it can be used again.
/// </summary>
public class MaskVision : MonoBehaviour
{
    // ── Inspector ───────────────────────────────────────────
    [Header("Meter")]
    public float maxMeter        = 3f;   // seconds of vision
    public float drainRate       = 1f;   // per second while held
    public float rechargeRate    = 0.4f; // per second while released
    [Tooltip("Meter must reach this fraction before vision can activate again.")]
    public float rechargeThreshold = 0.3f;

    // ── Runtime ─────────────────────────────────────────────
    public  float currentMeter;
    private bool  isActive   = false;
    private bool  isCharging = false;   // true while waiting for threshold

    // Cached references (set in Start)
    private MazeGenerator mazeGen;

    // ── Lifecycle ───────────────────────────────────────────
    private void Start()
    {
        currentMeter = maxMeter;
        mazeGen = FindFirstObjectByType<MazeGenerator>(); // Unity 6 API
    }

    // ── Every Frame ─────────────────────────────────────────
    private void Update()
    {
        bool held = Input.GetMouseButton(1); // Right Mouse Button

        if (held && !isCharging && currentMeter > 0f)
        {
            // ── ACTIVATE ──
            if (!isActive) ToggleVision(true);
            isActive = true;

            currentMeter -= drainRate * Time.deltaTime;
            if (currentMeter <= 0f)
            {
                currentMeter = 0f;
                ToggleVision(false);
                isActive   = false;
                isCharging = true;   // force full recharge
            }
        }
        else
        {
            // ── RELEASED / DEPLETED ──
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
                isCharging   = false;
            }

            // Check if we've passed the recharge threshold
            if (isCharging && currentMeter >= maxMeter * rechargeThreshold)
                isCharging = false;
        }
    }

    // ── Toggle all fake / invisible walls ───────────────────
    private void ToggleVision(bool on)
    {
        if (mazeGen == null) return;

        foreach (var fw in mazeGen.fakeWalls)
            fw.SetRevealed(on);

        foreach (var iw in mazeGen.invisWalls)
            iw.SetRevealed(on);
    }

    // ── Read-only meter ratio (used by GameManager HUD) ────
    public float MeterRatio => currentMeter / maxMeter;
}
