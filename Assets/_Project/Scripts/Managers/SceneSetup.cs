using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [Header("Multiplayer Only")]
    [SerializeField] GameObject player2;
    [SerializeField] GameObject cameraSystemP2;
    [SerializeField] GameObject splitLineCanvas;
    [SerializeField] GameObject raceTimerUI;
    [SerializeField] GameObject winPanel;

    [Header("Singleplayer Only")]
    [SerializeField] GameObject completePanel;
    [SerializeField] GameObject p2HealthBar;
    [SerializeField] GameObject bossEnemy;

    [Header("Cameras")]
    [SerializeField] Camera p1Camera;
    [SerializeField] Camera p2Camera;
    [SerializeField] GameObject p1DeathPanel;
[SerializeField] GameObject p2DeathPanel;

    void Start()
    {
        bool isMulti = false;

        if (GameModeManager.Instance != null)
        {
            isMulti = GameModeManager.CurrentMode ==
                GameModeManager.GameMode.MultiPlayer;
        }

        Debug.Log($"SceneSetup — isMulti: {isMulti}");
        ApplyMode(isMulti);
    }

    void ApplyMode(bool isMulti)
    {
        if (player2 != null) player2.SetActive(isMulti);
        if (cameraSystemP2 != null) cameraSystemP2.SetActive(isMulti);
        if (splitLineCanvas != null) splitLineCanvas.SetActive(isMulti);
        if (raceTimerUI != null) raceTimerUI.SetActive(isMulti);
        if (p2HealthBar != null) p2HealthBar.SetActive(isMulti);
        if (winPanel != null) winPanel.SetActive(false);
        if (completePanel != null) completePanel.SetActive(false);
        if (bossEnemy != null) bossEnemy.SetActive(!isMulti);

        if (isMulti)
        {
            if (p1Camera != null)
                p1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
            if (p2Camera != null)
                p2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
                Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (p2DeathPanel != null)
        p2DeathPanel.SetActive(false);
    }
        else
        {
            if (p1Camera != null)
                p1Camera.rect = new Rect(0f, 0f, 1f, 1f);
            if (p2Camera != null)
                p2Camera.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        }
    }
}