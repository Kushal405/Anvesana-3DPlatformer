using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode { SinglePlayer, MultiPlayer }
    public static GameMode CurrentMode = GameMode.SinglePlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}