using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Timer UI")]
    [SerializeField] TextMeshProUGUI timerText;

    [Header("Win Screen")]
    [SerializeField] GameObject winPanel;
    [SerializeField] TextMeshProUGUI winText;

    StopwatchTimer stopwatch;
    bool raceActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(gameObject); return; }
        Instance = this;

        GameModeManager.EnsureExists();
        stopwatch = new StopwatchTimer();
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);

        bool isMulti = GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;

        if (isMulti)
        {
            stopwatch.Start();
            raceActive = true;
            if (timerText != null)
                timerText.gameObject.SetActive(true);
            else
                Debug.LogError("RaceManager — timerText not assigned!");
        }
        else
        {
            raceActive = false;
            if (timerText != null)
                timerText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!raceActive) return;

        stopwatch.Tick(Time.deltaTime);

        float t   = stopwatch.GetTime();
        int mins  = (int)(t / 60f);
        int secs  = (int)(t % 60f);
        int ms    = (int)((t * 100f) % 100f);

        if (timerText != null)
            timerText.text = $"{mins:00}:{secs:00}:{ms:00}";
    }

    public void PlayerWon(int playerNumber)
    {
        if (!raceActive) return;
        raceActive = false;
        stopwatch.Pause();

        // Freeze game and show cursor for button clicks
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (winText  != null)
            winText.text = $"PLAYER {playerNumber} WINS!\n" +
                           $"Time: {timerText.text}";
    }

    // ── Called by Win Panel "Play Again" button ───────────────────────────
    public void RestartRace()
    {
        // Reset everything before scene reload
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (winPanel != null) winPanel.SetActive(false);

        // Reload scene — all players, flags, timer reset from scratch
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    // ── Called by Win Panel "Main Menu" button ────────────────────────────
    public void QuitToMenu()
    {
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (winPanel != null) winPanel.SetActive(false);

        SceneManager.LoadScene("MainMenu");
    }
}