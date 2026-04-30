using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : ValidatedMonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    int currentHealth;
    public UnityEngine.UI.Slider healthSlider;

    [Header("Death UI")]
    public GameObject deathPanel;

    [Header("Step / Slope Settings")]
    [SerializeField] float maxStepHeight = 0.4f;
    [SerializeField] float stepSmooth = 0.1f;
    [SerializeField] float maxSlopeAngle = 45f;

    [Header("References")]
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineCamera cinemachineFreeLook;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float rotationSpeed = 720f;

    // FIX: Use a speed value that maps to your blend tree thresholds (0, 0.5, 1)
    // We'll send 0.5 for walk and 1.0 for run (or just 0.5 always for walk)
    [SerializeField] float animWalkSpeed = 0.5f; // ← tune this to match threshold

    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int IsJumping = Animator.StringToHash("IsJumping");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DieHash = Animator.StringToHash("Die");

    Transform mainCam;
    Vector3 moveDir;
    bool isDead = false;

    int groundContactCount = 0;
    bool IsGrounded => groundContactCount > 0;

    void Awake()
    {
        mainCam = Camera.main.transform;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;


        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        var go = new GameObject("CameraTarget");
        var t = go.transform;
        t.SetParent(transform);
        t.localPosition = new Vector3(0f, 1.5f, 0f);
        cinemachineFreeLook.Target.TrackingTarget = t;
        cinemachineFreeLook.Target.CustomLookAtTarget = false;
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

        if (kb.wKey.isPressed) z = 1f;
        if (kb.sKey.isPressed) z = -1f;
        if (kb.aKey.isPressed) x = -1f;
        if (kb.dKey.isPressed) x = 1f;

        // FIX: Explicitly zero moveDir when no input — don't rely on magnitude check
        bool hasInput = (x != 0f || z != 0f);

        if (hasInput)
        {
            var camF = mainCam.forward; camF.y = 0f; camF.Normalize();
            var camR = mainCam.right; camR.y = 0f; camR.Normalize();
            moveDir = (camF * z + camR * x).normalized;
        }
        else
        {
            moveDir = Vector3.zero; // ← explicit zero, no lingering direction
        }

        // Rotation
        if (moveDir.sqrMagnitude > 0.01f)
        {
            var targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime);
        }

        // Jump
        if (kb.spaceKey.wasPressedThisFrame && IsGrounded && !isDead)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            groundContactCount = 0;
            animator.SetBool(IsJumping, true);
        }

        // Attack
        if (kb.fKey.wasPressedThisFrame && !isDead)
        {
            animator.SetTrigger(AttackHash);
            AttackNearbyEnemies();
        }

        // FIX: Send 0.5 for walk instead of raw moveDir.magnitude (which is 0 or 1)
        // This correctly hits your blend tree's Walk threshold at 0.5
        float targetAnimSpeed = hasInput ? animWalkSpeed : 0f;

        // Smooth the speed parameter so transitions aren't jarring
        float currentSpeed = animator.GetFloat(Speed);
        float smoothedSpeed = Mathf.MoveTowards(currentSpeed, targetAnimSpeed, Time.deltaTime * 10f);
        animator.SetFloat(Speed, smoothedSpeed);

        // Debug / test damage
        if (kb.tKey.wasPressedThisFrame && !isDead)
            TakeDamage(1);
    }

    void FixedUpdate()
{
    if (isDead) return;

    if (IsGrounded)
    {
        if (moveDir.sqrMagnitude > 0.01f)
        {
            // Project movement along the slope surface so uphill feels natural
            Vector3 slopeMove = ProjectOnSlope(moveDir);
            rb.linearVelocity = new Vector3(
                slopeMove.x * moveSpeed,
                slopeMove.y * moveSpeed, // ← allows Y to follow slope up/down
                slopeMove.z * moveSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    // Only add extra gravity in air — and keep it mild
    if (!IsGrounded)
    {
        rb.AddForce(Vector3.down * 8f, ForceMode.Acceleration);
    }
}

// Projects movement direction along the actual ground surface
Vector3 ProjectOnSlope(Vector3 direction)
{
    if (Physics.Raycast(transform.position + Vector3.up * 0.1f, 
        Vector3.down, out RaycastHit hit, 0.4f))
    {
        // Flatten movement along the slope normal
        return Vector3.ProjectOnPlane(direction, hit.normal).normalized;
    }
    return direction;
}

    // ── Health / Death (unchanged) ──────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthSlider != null) healthSlider.value = currentHealth;
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) StartCoroutine(DieSequence());
    }

    IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
        foreach (var r in renderers)
            r.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];
    }

    IEnumerator DieSequence()
    {
        isDead = true;
        animator.SetTrigger(DieHash);
        if (deathPanel != null) deathPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (deathPanel != null) deathPanel.SetActive(false);
        Respawn();
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        groundContactCount = 0;
        moveDir = Vector3.zero;
        if (healthSlider != null) healthSlider.value = currentHealth;
        transform.position = new Vector3(0f, 1f, 0f);
        rb.linearVelocity = Vector3.zero;
        animator.Rebind();
        animator.Update(0f);
        animator.SetBool(IsJumping, false);
        animator.SetFloat(Speed, 0f); // ← ensure clean idle on respawn
    }

    // ── Attack (unchanged) ─────────────────────────────────────────────────

    void AttackNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.TakeDamage(1);
        }
    }
}