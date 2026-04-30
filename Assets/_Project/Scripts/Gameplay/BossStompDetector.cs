using UnityEngine;
using System.Collections;

public class BossStompDetector : MonoBehaviour
{
    [Header("Stomp Settings")]
    public int stompDamage = 1;
    public float playerBounceForce = 9f;

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    BossGuardian boss;

    void Start()
    {
        boss = GetComponentInParent<BossGuardian>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody playerRb = other.GetComponent<Rigidbody>();

        // Only register as stomp if player is falling downward
        bool isFalling = playerRb != null && playerRb.linearVelocity.y < -0.5f;
        if (!isFalling) return;

        // Damage boss
        boss?.TakeDamage(stompDamage);

        // Bounce player up — satisfying Mario-style pop
        if (playerRb != null)
        {
            Vector3 vel = playerRb.linearVelocity;
            vel.y = playerBounceForce;
            playerRb.linearVelocity = vel;
        }

        StartCoroutine(StompFlash());
    }

    IEnumerator StompFlash()
    {
        var renderers = GetComponentsInParent<Renderer>();
        var origColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            origColors[i] = renderers[i].material.GetColor(BaseColor);

        foreach (var r in renderers)
            r.material.SetColor(BaseColor, Color.red);

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(BaseColor, origColors[i]);
    }
}