using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] Transform respawnPointP1;
    [SerializeField] Transform respawnPointP2;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPointP1.position;
        }
        else if (other.CompareTag("Player2"))
        {
            other.transform.position = respawnPointP2.position;
        }
    }
}