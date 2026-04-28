using UnityEngine;

public class Boulder : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        // Respawn player
        col.gameObject.transform.position = new Vector3(0f, 1f, 0f);
    }
}