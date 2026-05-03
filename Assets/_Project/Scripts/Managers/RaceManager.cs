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

    Debug.Log($"RaceManager — isMulti: {isMulti}");

    if (isMulti)
    {
        stopwatch.Start();
        raceActive = true;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            Debug.Log("Timer activated");
        }
        else
            Debug.LogError("Timer text not assigned!");
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

        float t = stopwatch.GetTime();
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        int ms = (int)((t * 100f) % 100f);

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