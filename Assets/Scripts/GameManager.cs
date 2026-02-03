using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that owns the game loop (generate → play → exit → repeat).
/// Also draws a simple on-screen HUD with legacy GUI showing:
///   • Mask Vision meter bar
///   • "Hold RMB for Mask Vision" hint
///   • Flash message when the exit is reached
/// No external UI framework needed — just OnGUI().
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ───────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Inspector ───────────────────────────────────────────
    [Header("Delay before regenerating after exit is reached (sec)")]
    public float restartDelay = 1.5f;

    // ── Runtime ─────────────────────────────────────────────
    private MazeGenerator  mazeGen;
    private PlayerController player;
    private MaskVision     maskVision;

    private bool  exitReached  = false;
    private float exitTimer    = 0f;

    // ── Lifecycle ───────────────────────────────────────────
    private void Awake()
    {
        // Simple singleton – only one GameManager should exist
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CacheReferences();
        StartRun();
    }

    // Called again after every regeneration
    private void CacheReferences()
    {
        mazeGen    = FindFirstObjectByType<MazeGenerator>();   // Unity 6
        player     = FindFirstObjectByType<PlayerController>();
        maskVision = FindFirstObjectByType<MaskVision>();
    }

    // ── Game Loop ───────────────────────────────────────────
    private void StartRun()
    {
        exitReached = false;

        if (mazeGen == null) return;

        mazeGen.Generate();                         // build maze
        player.Teleport(mazeGen.startPosition);     // place player
    }

    /// <summary>Called by ExitTrigger when the player reaches the exit.</summary>
    public void OnExitReached()
    {
        if (exitReached) return;   // ignore duplicate calls
        exitReached = true;
        exitTimer   = 0f;
    }

    private void Update()
    {
        if (!exitReached) return;

        exitTimer += Time.deltaTime;
        if (exitTimer >= restartDelay)
        {
            // Re-cache in case objects were recreated
            CacheReferences();
            StartRun();
        }
    }

    // ── HUD (legacy OnGUI – no packages required) ──────────
    private void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        // ── Mask Vision Meter Bar ───────────────────────────
        if (maskVision != null)
        {
            float ratio = maskVision.MeterRatio;

            // Background (dark bar)
            GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            GUI.Box(new Rect(20, 20, 200, 30), GUIContent.none);

            // Foreground (coloured fill)
            Color barColor = ratio > 0.4f
                ? new Color(0.3f, 0.8f, 0.3f)   // green – healthy
                : new Color(0.9f, 0.3f, 0.1f);  // red   – low

            GUI.color = barColor;
            GUI.Box(new Rect(20, 20, 200 * ratio, 30), GUIContent.none);

            // Label
            GUI.color = Color.white;
            GUI.Label(new Rect(25, 22, 190, 26),
                      "<b>KUTOB</b>  " + Mathf.CeilToInt(maskVision.currentMeter * 10) / 10f + "s");
        }

        // ── Hint text ───────────────────────────────────────
        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        GUI.Label(new Rect(20, 58, 300, 20), "Hold RMB → Mask Vision");

        // ── Exit reached flash ──────────────────────────────
        if (exitReached)
        {
            // Semi-transparent black overlay
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Box(new Rect(0, 0, w, h), GUIContent.none);

            // Big text in centre
            GUI.color = Color.white;
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize  = 48;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(w * 0.25f, h * 0.4f, w * 0.5f, 80),
                      "You felt it.", style);

            style.fontSize = 24;
            GUI.Label(new Rect(w * 0.3f, h * 0.55f, w * 0.4f, 40),
                      "Regenerating…", style);
        }

        // Reset GUI colour (good practice)
        GUI.color = Color.white;
    }
}
