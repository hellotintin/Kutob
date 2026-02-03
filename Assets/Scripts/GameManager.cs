using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Delay before regenerating after exit is reached (sec)")]
    public float restartDelay = 1.5f;

    private MazeGenerator  mazeGen;
    private PlayerController player;
    private MaskVision     maskVision;

    private bool  exitReached  = false;
    private float exitTimer    = 0f;

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
        CacheReferences();
        StartRun();
    }

    private void CacheReferences()
    {
        mazeGen    = FindFirstObjectByType<MazeGenerator>();  
        player     = FindFirstObjectByType<PlayerController>();
        maskVision = FindFirstObjectByType<MaskVision>();
    }

    // Game Loop
    private void StartRun()
    {
        exitReached = false;

        if (mazeGen == null) return;

        mazeGen.Generate();                         // build maze
        player.Teleport(mazeGen.startPosition);     // place player
    }

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
            CacheReferences();
            StartRun();
        }
    }

    private void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

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

        // Hint text 
        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        GUI.Label(new Rect(20, 58, 300, 20), "Hold RMB → Mask Vision");

        // Exit reached flash
        if (exitReached)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Box(new Rect(0, 0, w, h), GUIContent.none);

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

        GUI.color = Color.white;
    }
}
