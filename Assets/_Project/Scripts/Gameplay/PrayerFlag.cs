using UnityEngine;

public class PrayerFlag : MonoBehaviour
{
    [Header("Flag Settings")]
    [SerializeField] float rotateSpeed = 90f;
    [SerializeField] float bobSpeed = 1f;
    [SerializeField] float bobHeight = 0.3f;

    public enum FlagOwner { AnyPlayer, Player1Only, Player2Only }
    [SerializeField] FlagOwner flagOwner = FlagOwner.AnyPlayer;

    Vector3 startPos;
    bool collected = false;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        transform.position = startPos +
            Vector3.up * 
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        bool isP1 = other.CompareTag("Player");
        bool isP2 = other.CompareTag("Player2");

        if (!isP1 && !isP2) return;

        // Check ownership
        if (flagOwner == FlagOwner.Player1Only && !isP1) return;
        if (flagOwner == FlagOwner.Player2Only && !isP2) return;

        collected = true;
        AudioManager.Instance?.PlayFlag();

        if (isP1)
            GameManager.Instance?.CollectFlagP1();
        else
            GameManager.Instance?.CollectFlagP2();

        gameObject.SetActive(false);
        Debug.Log($"Flag collected by {other.tag}");
    }
}