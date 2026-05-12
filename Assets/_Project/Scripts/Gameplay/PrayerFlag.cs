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
       Debug.Log("TRIGGER ENTERED by: " + other.name);

    if (collected)
    {
        Debug.Log("Already collected");
        return;
    }

    bool isP1 = other.CompareTag("Player");
    bool isP2 = other.CompareTag("Player2");

    Debug.Log("Player tag check:");
    Debug.Log("isP1 = " + isP1);
    Debug.Log("isP2 = " + isP2);
    Debug.Log("Actual Tag = " + other.tag);

    if (!isP1 && !isP2)
    {
        Debug.Log("Not a valid player");
        return;
    }

    // Ownership check
    Debug.Log("Flag Owner = " + flagOwner);

    if (flagOwner == FlagOwner.Player1Only && !isP1)
    {
        Debug.Log("P2 tried collecting P1 flag");
        return;
    }

    if (flagOwner == FlagOwner.Player2Only && !isP2)
    {
        Debug.Log("P1 tried collecting P2 flag");
        return;
    }

    collected = true;

    Debug.Log("COLLECTION SUCCESS");

    AudioManager.Instance?.PlayFlag();

    if (isP1)
    {
        Debug.Log("Calling CollectFlagP1()");
        GameManager.Instance?.CollectFlagP1();
    }
    else
    {
        Debug.Log("Calling CollectFlagP2()");
        GameManager.Instance?.CollectFlagP2();
    }

    gameObject.SetActive(false);

    Debug.Log($"Flag collected by {other.tag}");
} 
    //     if (collected) return;

    //     bool isP1 = other.CompareTag("Player");
    //     bool isP2 = other.CompareTag("Player2");

    //     if (!isP1 && !isP2) return;

    //     // Check ownership
    //     if (flagOwner == FlagOwner.Player1Only && !isP1) return;
    //     if (flagOwner == FlagOwner.Player2Only && !isP2) return;

    //     collected = true;
    //     AudioManager.Instance?.PlayFlag();

    //     if (isP1)
    //         GameManager.Instance?.CollectFlagP1();
    //     else
    //         GameManager.Instance?.CollectFlagP2();

    //     gameObject.SetActive(false);
    //     Debug.Log($"Flag collected by {other.tag}");
    // }
}