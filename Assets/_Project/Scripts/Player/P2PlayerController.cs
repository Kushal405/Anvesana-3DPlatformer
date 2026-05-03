using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class P2PlayerController : ValidatedMonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    int currentHealth;
    public UnityEngine.UI.Slider healthSlider;

    [Header("Death UI")]
    public GameObject deathPanel;

    [Header("Respawn")]
    [SerializeField] Transform respawnPoint;

    [Header("References")]
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineCamera cinemachineFreeLook;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float rotationSpeed = 720f;
    [SerializeField] float animWalkSpeed = 0.5f;

    [Header("Camera")]
    [SerializeField] Camera p2Camera;

    static readonly int Speed      = Animator.StringToHash("Speed");
    static readonly int IsJumping  = Animator.StringToHash("IsJumping");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DieHash    = Animator.StringToHash("Die");
    static readonly int GetHitHash = Animator.StringToHash("GetHit");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    Transform mainCam;
    Vector3 moveDir;
    bool isDead = false;
    CinemachineInputAxisController cinemachineInput;

    int groundContactCount = 0;
    bool IsGrounded => groundContactCount > 0;

    void Awake()
    {
        mainCam = p2Camera != null ?
            p2Camera.transform : Camera.main.transform;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        animator.applyRootMotion = false;

        var go = new GameObject("CameraTarget_P2");
        var t = go.transform;
        t.SetParent(transform);
        t.localPosition = new Vector3(0f, 1.5f, 0f);
        cinemachineFreeLook.Target.TrackingTarget = t;
        cinemachineFreeLook.Target.CustomLookAtTarget = false;
        cinemachineInput = cinemachineFreeLook
            .GetComponent<CinemachineInputAxisController>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    void OnCollisionEnter(Collision col)
    {
        foreach (var contact in col.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundContactCount++;
                animator.SetBool(IsJumping, false);
                break;
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        groundContactCount = Mathf.Max(0, groundContactCount - 1);
    }

    void Update()
    {
        if (isDead) return;

        var kb = Keyboard.current;
        float x = 0f, z = 0f;

        if (kb.upArrowKey.isPressed)    z =  1f;
        if (kb.downArrowKey.isPressed)  z = -1f;
        if (kb.leftArrowKey.isPressed)  x = -1f;
        if (kb.rightArrowKey.isPressed) x =  1f;

        bool hasInput = (x != 0f || z != 0f);

        if (hasInput)
        {
            var camF = mainCam.forward; camF.y = 0f; camF.Normalize();
            var camR = mainCam.right;   camR.y = 0f; camR.Normalize();
            moveDir = (camF * z + camR * x).normalized;
        }
        else
        {
            moveDir = Vector3.zero;
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            var targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot,
                rotationSpeed * Time.deltaTime);
        }

        // Jump
        if (kb.periodKey.wasPressedThisFrame && IsGrounded && !isDead)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            groundContactCount = 0;
            animator.SetBool(IsJumping, true);
        }

        if (kb.slashKey.wasPressedThisFrame && !isDead)
        {
            animator.SetTrigger(AttackHash);
            AttackNearbyEnemies();
        }

        float targetAnimSpeed = hasInput ? animWalkSpeed : 0f;
        float currentSpeed = animator.GetFloat(Speed);
        animator.SetFloat(Speed,
            Mathf.MoveTowards(currentSpeed, targetAnimSpeed,
                Time.deltaTime * 10f));
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (IsGrounded)
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 slopeMove = ProjectOnSlope(moveDir);
                rb.linearVelocity = new Vector3(
                    slopeMove.x * moveSpeed,
                    slopeMove.y * moveSpeed,
                    slopeMove.z * moveSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    0f, rb.linearVelocity.y, 0f);
            }
        }

        if (!IsGrounded)
            rb.AddForce(Vector3.down * 8f, ForceMode.Acceleration);
    }

    Vector3 ProjectOnSlope(Vector3 direction)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
            Vector3.down, out RaycastHit hit, 0.4f))
            return Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        return direction;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthSlider != null) healthSlider.value = currentHealth;
        animator.SetTrigger(GetHitHash);
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) StartCoroutine(DieSequence());
    }

    IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.GetColor(BaseColorID);
        foreach (var r in renderers)
            r.material.SetColor(BaseColorID, Color.red);
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(BaseColorID, originalColors[i]);
    }

    IEnumerator DieSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        animator.SetTrigger(DieHash);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cinemachineInput != null)
            cinemachineInput.enabled = false;

        yield return new WaitForSeconds(1.5f);
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    public void Respawn()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        StopAllCoroutines();

        isDead = false;
        currentHealth = maxHealth;
        groundContactCount = 0;
        moveDir = Vector3.zero;

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (respawnPoint != null)
            transform.position = respawnPoint.position;
        else
            transform.position = new Vector3(2f, 1f, 0f);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;

        animator.ResetTrigger(GetHitHash);
        animator.ResetTrigger(DieHash);
        animator.ResetTrigger(AttackHash);
        animator.Rebind();
        animator.Update(0f);
        animator.applyRootMotion = false;
        animator.SetBool(IsJumping, false);
        animator.SetFloat(Speed, 0f);

        StartCoroutine(ReenableInput());
    }

    IEnumerator ReenableInput()
    {
        yield return null;
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cinemachineInput != null)
            cinemachineInput.enabled = true;
    }

    public void QuitGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager
            .LoadScene("MainMenu");
    }

    void AttackNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 2f);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null) { enemy.TakeDamage(1); continue; }
            var boss = hit.GetComponent<BossGuardian>();
            if (boss != null) boss.TakeDamage(1);
        }
    }
}