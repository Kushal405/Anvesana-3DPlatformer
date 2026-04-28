using UnityEngine;

public class PrayerFlag : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 90f;
    [SerializeField] float bobSpeed = 1f;
    [SerializeField] float bobHeight = 0.3f;

    Vector3 startPos;
    bool collected = false; 

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        transform.position = startPos +
            Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return; 
        if (!other.CompareTag("Player")) return;
        collected = true;
        GameManager.Instance.CollectFlag();
        gameObject.SetActive(false);
    }
}