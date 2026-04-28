using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.transform.position = respawnPoint.position;
    }
}
