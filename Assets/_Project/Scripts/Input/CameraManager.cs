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
            orbitalFollow = freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
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
            isRightMousePressed = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            StartCoroutine(DisableMouseForOneFrame());
        }

        void OnDisableMouseControl()
        {
            isRightMousePressed = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Reset axes to prevent snapping
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
            if (isDeviceMouse && !isRightMousePressed) return;

            float deviceMultiplier = isMouseDevice ? Time.fixedDeltaTime : 1f;
            orbitalFollow.HorizontalAxis.Value += cameraMovement.x * speed * deviceMultiplier;
            orbitalFollow.VerticalAxis.Value += cameraMovement.y * speed * deviceMultiplier;
        }
    }
