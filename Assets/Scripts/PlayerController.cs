using UnityEngine;

/// <summary>
/// Simple first-person controller.
/// Uses legacy Input.GetAxis — works in Unity 6 if you set
/// Edit ▸ Project Settings ▸ Input Handling ▸ "Both".
///
/// Movement : W A S D
/// Look     : Mouse
/// The player is a Capsule with a Rigidbody (no gravity needed
/// since we move on a flat Y=0 plane; gravity is disabled).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    // ── Inspector ───────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Look")]
    public float lookSensitivity = 2f;
    [Tooltip("Clamp vertical look so the camera doesn't flip.")]
    public float maxLookAngle = 80f;

    // ── Private ─────────────────────────────────────────────
    private Rigidbody rb;
    private float verticalRotation = 0f;   // accumulated pitch

    // ── Lifecycle ───────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // We handle rotation manually → freeze all Rigidbody rotation
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // We live on a flat plane; no gravity needed
        rb.useGravity = false;
    }

    private void Start()
    {
        // Lock cursor to centre of screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // ── Every Frame ─────────────────────────────────────────
    private void Update()
    {
        // --- Mouse look (runs in Update for smooth feel) ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Horizontal rotation (yaw) – rotate the player GameObject
        transform.Rotate(0f, mouseX * lookSensitivity, 0f);

        // Vertical rotation (pitch) – rotate only the Camera child
        verticalRotation -= mouseY * lookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        // Apply pitch to the camera (assumed to be the first child Camera)
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        // --- WASD movement (physics-based, runs in FixedUpdate) ---
        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical   = Input.GetAxis("Vertical");   // W / S

        // Move relative to the direction the player is facing
        Vector3 moveDir = transform.right   * horizontal +
                          transform.forward * vertical;

        // Normalise so diagonal movement isn't faster
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        // Set velocity directly (no acceleration; crisp feel)
        rb.linearVelocity = moveDir * moveSpeed;
    }

    // ── Utility ─────────────────────────────────────────────
    /// <summary>Teleport the player to a world position (used on restart).</summary>
    public void Teleport(Vector3 pos)
    {
        // Offset Y so the capsule centre sits above the floor
        rb.MovePosition(pos + Vector3.up * 0.5f);
        rb.linearVelocity = Vector3.zero;

        // Reset look
        verticalRotation = 0f;
        transform.rotation = Quaternion.identity;
    }
}
