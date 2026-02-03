using UnityEngine;
public class FakeWall : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor   = new Color(0.55f, 0.55f, 0.55f, 1.0f); // greyyy
    public Color revealedColor = new Color(1.0f,  0.2f,  0.2f, 0.4f);  // reddd, transparent
    private Material mat;
    private BoxCollider box;
    private void Awake()
    {
        // create unique material so color changes don't affect every other cube
        var renderer = GetComponent<Renderer>();
        mat = renderer.material;          
        mat.color = normalColor;

        // disable the collider so the player phases through
        box = GetComponent<BoxCollider>();
        if (box != null)
            box.enabled = false;
    }


    /// true = show player that the wall is fake (red + transparent)
    /// false = hide the truth (go back to normal grey)
    public void SetRevealed(bool revealed)
    {
        mat.color = revealed ? revealedColor : normalColor;
    }
}
