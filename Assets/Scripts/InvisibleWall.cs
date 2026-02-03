using UnityEngine;

public class InvisibleWall : MonoBehaviour
{
    [Header("Revealed Color")]
    public Color revealedColor = new Color(0.2f, 0.4f, 1.0f, 0.7f); 
    private MeshRenderer meshRenderer;
    private Material     mat;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // duplicate da material so don't needa tint every cube
        mat = meshRenderer.material;
        mat.color = revealedColor;

        // START invisible  
        meshRenderer.enabled = false;

        var box = GetComponent<BoxCollider>();
        if (box != null)
            box.enabled = true;
    }

    //(called by MaskVision) 
    public void SetRevealed(bool revealed)
    {
        meshRenderer.enabled = revealed;
    }
}
