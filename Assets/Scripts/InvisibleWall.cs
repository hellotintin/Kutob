using UnityEngine;

/// <summary>
/// A wall you CANNOT see but CANNOT walk through.
/// Normal state  : MeshRenderer disabled, BoxCollider enabled.
/// Revealed state: MeshRenderer ON, glowing BLUE so the player knows it's there.
///
/// Setup: Attach to a Cube. The script disables the renderer on Awake.
/// </summary>
public class InvisibleWall : MonoBehaviour
{
    // ── Inspector ───────────────────────────────────────────
    [Header("Revealed Color")]
    public Color revealedColor = new Color(0.2f, 0.4f, 1.0f, 0.7f); // blue, slightly transparent

    // ── Private ─────────────────────────────────────────────
    private MeshRenderer meshRenderer;
    private Material     mat;

    // ── Lifecycle ───────────────────────────────────────────
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Duplicate the material so we don't tint every cube
        mat = meshRenderer.material;
        mat.color = revealedColor;

        // START invisible (this is the whole trick)
        meshRenderer.enabled = false;

        // Make sure the collider IS enabled — this blocks the player
        var box = GetComponent<BoxCollider>();
        if (box != null)
            box.enabled = true;
    }

    // ── Public API (called by MaskVision) ───────────────────
    /// <summary>
    /// true  → flash the hidden wall visible (blue)
    /// false → hide it again
    /// </summary>
    public void SetRevealed(bool revealed)
    {
        meshRenderer.enabled = revealed;
    }
}
