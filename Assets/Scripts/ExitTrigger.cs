using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<BoxCollider>();
        if (col != null)
            col.isTrigger = true;

        var mat = GetComponent<Renderer>().material;
        mat.color = new Color(0.1f, 0.9f, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnExitReached();
        }
    }
}
