using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Flags")]
    [SerializeField] int requiredFlags = 4;
    [SerializeField] TextMeshProUGUI p1FlagText;
    [SerializeField] TextMeshProUGUI p2FlagText;
    [SerializeField] MonkHead monkHead;

    [Header("Boss")]
    public GameObject zone4Gate;

    int p1Flags = 0;
    int p2Flags = 0;
    bool monkAwakened = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        UpdateUI();
    }

    void UpdateUI()
    {
        bool isMulti = GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;

        if (isMulti)
        {
            if (p1FlagText != null)
                p1FlagText.text = $"Flags: {p1Flags}/{requiredFlags}";
            if (p2FlagText != null)
                p2FlagText.text = $"Flags: {p2Flags}/{requiredFlags}";
        }
        else
        {
            if (p1FlagText != null)
                p1FlagText.text = 
                    $"Prayer Flags: {p1Flags}/{requiredFlags}";
            if (p2FlagText != null)
                p2FlagText.gameObject.SetActive(false);
        }
    }

    public void CollectFlagP1()
    {
        if (p1Flags >= requiredFlags) return;
        p1Flags++;
        UpdateUI();
        AudioManager.Instance?.PlayFlag();
        Debug.Log($"P1 flags: {p1Flags}/{requiredFlags}");
        CheckAwaken();
    }

    public void CollectFlagP2()
    {
        if (p2Flags >= requiredFlags) return;
        p2Flags++;
        UpdateUI();
        AudioManager.Instance?.PlayFlag();
        Debug.Log($"P2 flags: {p2Flags}/{requiredFlags}");
        CheckAwaken();
    }

    public void CollectFlag() => CollectFlagP1();

    void CheckAwaken()
    {
        bool isMulti = GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;

        // Singleplayer: P1 collects all 4
        // Multiplayer: EITHER player collects all 4
        bool p1Done = p1Flags >= requiredFlags;
        bool p2Done = p2Flags >= requiredFlags;
        bool shouldAwaken = isMulti ? 
            (p1Done || p2Done) : p1Done;

        if (shouldAwaken && !monkAwakened)
        {
            monkAwakened = true;
            Debug.Log("All flags collected! Monk awakens.");
            monkHead?.Awaken();
        }
    }

    public void OnBossDefeated()
    {
        Debug.Log("Boss defeated! Zone 4 unlocked.");
        zone4Gate?.GetComponent<SacredGate>()?.OpenGate();
    }
}