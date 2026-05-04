using System.Collections;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : ValidatedMonoBehaviour
{
    [SerializeField] InputReader inputReader;
    [SerializeField] CinemachineCamera freeLookCamera;
    [SerializeField] float speed = 1f;
    [SerializeField] float mouseSensitivity = 1.5f;
    [SerializeField] float touchSensitivity = 0.05f;

    bool isRightMousePressed;
    bool isDeviceMouse;
    CinemachineOrbitalFollow orbitalFollow;

    void Awake()
    {
        orbitalFollow = freeLookCamera
            .GetComponent<CinemachineOrbitalFollow>();
    }

    void Start()
    {
        GameModeManager.EnsureExists();

        if (orbitalFollow != null)
        {
            orbitalFollow.HorizontalAxis.Value = 0;
            orbitalFollow.VerticalAxis.Value = 0;
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
        if (IsMultiplayer()) return;

        isRightMousePressed = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(DisableMouseForOneFrame());
    }

    void OnDisableMouseControl()
    {
        if (IsMultiplayer()) return;

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
        if (IsMultiplayer() && isMouseDevice) return;
        if (isMouseDevice && !isRightMousePressed) return;

        float sensitivity = isMouseDevice ? mouseSensitivity : touchSensitivity;

        // Normalize mouse vs touch behavior
        float deltaMultiplier = isMouseDevice ? Time.deltaTime : 1f;

        Vector2 finalInput = cameraMovement * sensitivity * deltaMultiplier;

        orbitalFollow.HorizontalAxis.Value += finalInput.x;
        orbitalFollow.VerticalAxis.Value += finalInput.y;
    }

    bool IsMultiplayer()
    {
        return GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;
    }
}