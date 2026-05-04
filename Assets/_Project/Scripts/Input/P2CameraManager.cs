using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

// Simplified camera for P2 — fixed follow, no rotation input needed

public class P2CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera freeLookCamera;

    CinemachineOrbitalFollow orbitalFollow;

    void Awake()
    {
        orbitalFollow = freeLookCamera
            .GetComponent<CinemachineOrbitalFollow>();

        // Lock camera behind player — no rotation
        if (orbitalFollow != null)
        {
            orbitalFollow.HorizontalAxis.Value = 0;
            orbitalFollow.VerticalAxis.Value = 0;
        }
    }
}