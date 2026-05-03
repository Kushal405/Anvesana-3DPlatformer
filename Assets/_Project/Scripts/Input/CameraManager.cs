using System.Collections;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : ValidatedMonoBehaviour
{
    [SerializeField] InputReader inputReader;
    [SerializeField] CinemachineCamera freeLookCamera;
    [SerializeField] float speed = 1f;

    bool isRightMousePressed;
    bool isDeviceMouse;
    CinemachineOrbitalFollow orbitalFollow;

    bool isMultiplayer = false;

    void Awake()
    {
        orbitalFollow = freeLookCamera
            .GetComponent<CinemachineOrbitalFollow>();
    }

    void Start()
    {
        GameModeManager.EnsureExists();
        
        isMultiplayer = GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;

        Debug.Log($"CameraManager — isMultiplayer: {isMultiplayer}");

        // In multiplayer lock cursor immediately
        // no right click needed
        if (!isMultiplayer)
        {
            // Singleplayer starts with cursor locked
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnEnable()
    {
        inputReader.Look += OnLook;
        inputReader.EnableMouseControlCamera += OnEnableMouseControl;
        inputReader.DisableMouseControlCamera += OnDisableMouseControl;
    }

    void OnDisable()
    {
        inputReader.Look -= OnLook;
        inputReader.EnableMouseControlCamera -= OnEnableMouseControl;
        inputReader.DisableMouseControlCamera -= OnDisableMouseControl;
    }

    void OnEnableMouseControl()
    {
        if (isMultiplayer) return;

        isRightMousePressed = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(DisableMouseForOneFrame());
    }

    void OnDisableMouseControl()
    {
        if (isMultiplayer) return;

        isRightMousePressed = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        orbitalFollow.HorizontalAxis.Value = 0;
        orbitalFollow.VerticalAxis.Value = 0;
    }

    IEnumerator DisableMouseForOneFrame()
    {
        yield return null;
    }

    void OnLook(Vector2 cameraMovement, bool isMouseDevice)
    {
        if (isMultiplayer) return;

        isDeviceMouse = isMouseDevice;
        if (isDeviceMouse && !isRightMousePressed) return;

        float deviceMultiplier = isMouseDevice ?
            Time.fixedDeltaTime : 1f;
        orbitalFollow.HorizontalAxis.Value +=
            cameraMovement.x * speed * deviceMultiplier;
        orbitalFollow.VerticalAxis.Value +=
            cameraMovement.y * speed * deviceMultiplier;
    }
}