using UnityEngine;

/// <summary>
/// Placed on the Exit cube.  When the player enters the trigger volume
/// GameManager.Instance.OnExitReached() is called → maze regenerates.
///
/// The exit cube is a bright green cube so it's visible at the end of
/// the corridor. It has isTrigger = true on its collider.
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    private void Awake()
    {
        // Ensure this cube acts as a trigger (no physical collision)
        var col = GetComponent<BoxCollider>();
        if (col != null)
            col.isTrigger = true;

        // Paint it green
        var mat = GetComponent<Renderer>().material;
        mat.color = new Color(0.1f, 0.9f, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react to the Player
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnExitReached();
        }
    }
}
