using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] int requiredFlags = 4;
    [SerializeField] TextMeshProUGUI flagCountText;
    [SerializeField] MonkHead monkHead;

    int collectedFlags = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Initialize text
        flagCountText.text = $"Prayer Flags: 0/{requiredFlags}";
    }

    public void CollectFlag()
    {
        if (collectedFlags >= requiredFlags) return; // ← prevent over counting
        
        collectedFlags++;
        flagCountText.text = $"Prayer Flags: {collectedFlags}/{requiredFlags}";
        Debug.Log($"Flag collected: {collectedFlags}/{requiredFlags}");

        if (collectedFlags >= requiredFlags)
        {
            Debug.Log("All flags collected!");
            monkHead?.Awaken();
        }
    }
}