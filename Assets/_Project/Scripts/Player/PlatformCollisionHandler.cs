using UnityEngine;

public class PlatformCollisionHandler : MonoBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("MovingPlatform")) return;
        if (collision.contacts[0].normal.y < 0.5f) return;
        
        // Don't parent — instead kinematically follow
        transform.SetParent(collision.transform);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("MovingPlatform")) return;
        transform.SetParent(null);
    }
}