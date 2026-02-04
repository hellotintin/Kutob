using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Delay before regenerating after exit is reached (sec)")]
    public float restartDelay = 1.5f;

    private MazeGenerator mazeGen;
    private PlayerController player;
    private MaskVision maskVision;

    private bool exitReached = false;
    private float exitTimer = 0f;

    private void Awake()
    {
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
        StartRun();
    }

    /// <summary>
    /// Safely cache references. Returns true if all required refs exist.
    /// </summary>
    private bool CacheReferences()
    {
        mazeGen = FindFirstObjectByType<MazeGenerator>();
        player = FindFirstObjectByType<PlayerController>();
        maskVision = FindFirstObjectByType<MaskVision>();

        if (mazeGen == null) Debug.LogWarning("MazeGenerator not found in scene!");
        if (player == null) Debug.LogWarning("PlayerController not found in scene!");
        if (maskVision == null) Debug.LogWarning("MaskVision not found in scene!");

        return mazeGen != null && player != null;
    }


    /// Start or restart a maze run

    private void StartRun()
    {
        exitReached = false;

        if (!CacheReferences()) return;  

        mazeGen.Generate();                         // build maze

        // safe teleport: check player
        if (player != null)
            player.Teleport(mazeGen.startPosition);
    }

    public void OnExitReached()
    {
        if (exitReached) return;
        exitReached = true;
        exitTimer = 0f;
    }

    private void Update()
    {
        if (!exitReached) return;

        exitTimer += Time.deltaTime;
        if (exitTimer >= restartDelay)
        {
            StartRun();  // re-run safely
        }
    }

    private void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle largeMeterStyle = new GUIStyle(GUI.skin.label);
        largeMeterStyle.fontSize = 20; // Bigger font size
        largeMeterStyle.fontStyle = FontStyle.Bold;
        largeMeterStyle.normal.textColor = Color.white;

        GUIStyle largeHintStyle = new GUIStyle(GUI.skin.label);
        largeHintStyle.fontSize = 16; // Bigger font size
        largeHintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);

        if (maskVision != null)
        {
            float ratio = maskVision.MeterRatio;

            // Background (dark bar) - increased height for larger text
            GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            GUI.Box(new Rect(20, 20, 200, 40), GUIContent.none);

            // Foreground (coloured fill)
            Color barColor = ratio > 0.4f
                ? new Color(0.3f, 0.8f, 0.3f)   // green – healthy
                : new Color(0.9f, 0.3f, 0.1f);  // red   – low

            GUI.color = barColor;
            GUI.Box(new Rect(20, 20, 200 * ratio, 40), GUIContent.none);

            // Label with larger font
            GUI.color = Color.white;
            GUI.Label(new Rect(25, 22, 190, 36),
                      "<b>KUTOB</b>  " + Mathf.CeilToInt(maskVision.currentMeter * 10) / 10f + "s",
                      largeMeterStyle);
        }

        GUI.Label(new Rect(20, 65, 300, 25),
                  "Hold RMB → Mask Vision",
                  largeHintStyle);
    }
}
