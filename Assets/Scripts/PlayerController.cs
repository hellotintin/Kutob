using UnityEngine;
using UnityEngine.InputSystem;  

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float lookSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Rigidbody rb;
    private float verticalRotation = 0f;
    
    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // Mouse look
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        // Horizontal rotation
        transform.Rotate(0f, mouseX, 0f);

        // Vertical rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        // WASD movement
        Vector3 moveDir = transform.right * moveInput.x +
                         transform.forward * moveInput.y;

        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        rb.linearVelocity = moveDir * moveSpeed;
    }

    public void Teleport(Vector3 pos)
    {
        rb.MovePosition(pos + Vector3.up * 0.5f);
        rb.linearVelocity = Vector3.zero;
        verticalRotation = 0f;
        transform.rotation = Quaternion.identity;
    }
}