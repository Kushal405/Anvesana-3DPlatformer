using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Flags")]
    [SerializeField] int requiredFlags = 4;
    [SerializeField] TextMeshProUGUI flagCountText;
    [SerializeField] MonkHead monkHead;

    [Header("Boss")]
    public GameObject zone4Gate;

    int collectedFlags = 0;
    bool bossDefeated = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (flagCountText != null)
            flagCountText.text = $"Prayer Flags: 0/{requiredFlags}";
    }

    public void CollectFlag()
    {
        if (collectedFlags >= requiredFlags) return;

        collectedFlags++;

        if (flagCountText != null)
            flagCountText.text = $"Prayer Flags: {collectedFlags}/{requiredFlags}";

        Debug.Log($"Flag collected: {collectedFlags}/{requiredFlags}");

        if (collectedFlags >= requiredFlags)
        {
            Debug.Log("All flags collected!");
            monkHead?.Awaken();
        }
    }

    public void OnBossDefeated()
    {
        if (bossDefeated) return; // guard against calling twice
        bossDefeated = true;

        Debug.Log("Boss defeated! Zone 4 path unlocked.");
        zone4Gate?.GetComponent<SacredGate>()?.OpenGate();
    }
}