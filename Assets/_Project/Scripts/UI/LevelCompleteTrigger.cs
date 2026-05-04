using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI completeText;
    [SerializeField] GameObject completPanel;
    bool completed = false;
    public bool IsCompleted => completed;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (completed) return;
        completed = true;

        if (completPanel != null)
            completPanel.SetActive(true);

        if (completeText != null)
            completeText.text = "You Reached Swayambhu!\nLevel Complete!";

        // Only freeze if panel is actually visible
        if (completPanel != null && completPanel.activeInHierarchy)
        {
            Time.timeScale   = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ── "Play Again" button ───────────────────────────────────────────────
    public void PlayAgain()
    {
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        completed        = false;

        if (completPanel != null)
            completPanel.SetActive(false);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    // ── "Main Menu" button ────────────────────────────────────────────────
    public void GoToMainMenu()
    {
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        completed        = false;

        if (completPanel != null)
            completPanel.SetActive(false);

        SceneManager.LoadScene("MainMenu");
    }

    // ── Called by PlayerController.Respawn() only when IsCompleted = true ─
    public void ForceReset()
    {
        Time.timeScale = 1f;
        completed      = false;

        if (completPanel != null)
            completPanel.SetActive(false);
    }
}