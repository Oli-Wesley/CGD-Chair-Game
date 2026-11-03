using UnityEngine;

public class IcySlope : MonoBehaviour
{
    [SerializeField] private float gravityInside = 1f;
    private float gravityOutside = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.attachedRigidbody != null)
        {
            other.attachedRigidbody.gravityScale = gravityInside;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.attachedRigidbody != null)
        {
            other.attachedRigidbody.gravityScale = gravityOutside;
        }
    }
}
