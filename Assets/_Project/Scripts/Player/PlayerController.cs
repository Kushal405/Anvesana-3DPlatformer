using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : ValidatedMonoBehaviour
{
    [Header("References")]
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineCamera cinemachineFreeLook;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float rotationSpeed = 720f;

    static readonly int Speed = Animator.StringToHash("Speed");
    Transform mainCam;
    bool isGrounded;
    Vector3 moveDir;

    void Awake()
    {
        mainCam = Camera.main.transform;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var go = new GameObject("CameraTarget");
        var t = go.transform;
        t.SetParent(transform);
        t.localPosition = new Vector3(0f, 1.5f, 0f);
        cinemachineFreeLook.Target.TrackingTarget = t;
        cinemachineFreeLook.Target.CustomLookAtTarget = false;
    }

    void OnCollisionStay(Collision col) => isGrounded = true;
    void OnCollisionExit(Collision col) => isGrounded = false;

    void Update()
    {
        var kb = Keyboard.current;
        float x = 0f, z = 0f;

        if (kb.wKey.isPressed) z = 1f;
        if (kb.sKey.isPressed) z = -1f;
        if (kb.aKey.isPressed) x = -1f;
        if (kb.dKey.isPressed) x = 1f;

        var camF = mainCam.forward; camF.y = 0f; camF.Normalize();
        var camR = mainCam.right; camR.y = 0f; camR.Normalize();
        moveDir = (camF * z + camR * x).normalized;

        // Rotation in Update — smoother than FixedUpdate
        if (moveDir.magnitude > 0.1f)
        {
            var targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Jump
        if (kb.spaceKey.wasPressedThisFrame && isGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Animator
        animator.SetFloat(Speed, moveDir.magnitude);
    }

    void FixedUpdate()
    {
        // Movement only in FixedUpdate — no rotation here
        if (moveDir.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}