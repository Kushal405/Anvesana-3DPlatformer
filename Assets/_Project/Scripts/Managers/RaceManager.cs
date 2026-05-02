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

        stopwatch = new StopwatchTimer();
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);

        // Only run timer in multiplayer
        bool isMulti = GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;

        if (isMulti)
        {
            stopwatch.Start();
            raceActive = true;
        }
        else
        {
            // Hide timer in singleplayer
            if (timerText != null)
                timerText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!raceActive) return;

        stopwatch.Tick(Time.deltaTime);

        // Format MM:SS:ms
        float t = stopwatch.GetTime();
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        int ms      = (int)((t * 100f) % 100f);

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}:{ms:00}";
    }

    public void PlayerWon(int playerNumber)
    {
        if (!raceActive) return;
        raceActive = false;
        stopwatch.Pause();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (winText != null)
            winText.text = $"PLAYER {playerNumber} WINS!\n" +
                           $"Time: {timerText.text}";
    }

    public void RestartRace()
    {
        stopwatch.Reset();
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}