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
        isDeviceMouse = isMouseDevice;

        if (IsMultiplayer() && isMouseDevice) return;
        if (isDeviceMouse && !isRightMousePressed) return;

        float deviceMultiplier = isMouseDevice ?
            Time.fixedDeltaTime : 1f;
        orbitalFollow.HorizontalAxis.Value +=
            cameraMovement.x * speed * deviceMultiplier;
        orbitalFollow.VerticalAxis.Value +=
            cameraMovement.y * speed * deviceMultiplier;
    }

    bool IsMultiplayer()
    {
        return GameModeManager.Instance != null &&
            GameModeManager.CurrentMode ==
            GameModeManager.GameMode.MultiPlayer;
    }
}