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

    [Header("Air Control")]
    [SerializeField] float airControl = 0.3f;
    [SerializeField] float airDamping = 0.98f;

    [Header("Death UI")]
    public GameObject deathPanel;

    [Header("Respawn")]
    [SerializeField] Transform respawnPoint;

    [Header("References")]
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineCamera cinemachineFreeLook;

    [Header("Player 2 Camera")]
    [SerializeField] Camera p2Camera;

    [Header("Settings")]
    [SerializeField] float moveSpeed     = 3f;
    [SerializeField] float jumpForce     = 6f;
    [SerializeField] float rotationSpeed = 720f;
    [SerializeField] float animWalkSpeed = 0.5f;

    static readonly int Speed       = Animator.StringToHash("Speed");
    static readonly int IsJumping   = Animator.StringToHash("IsJumping");
    static readonly int AttackHash  = Animator.StringToHash("Attack");
    static readonly int DieHash     = Animator.StringToHash("Die");
    static readonly int GetHitHash  = Animator.StringToHash("GetHit");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    Transform mainCam;
    Vector3 moveDir;
    bool isDead = false;
    CinemachineInputAxisController cinemachineInput;

    int groundContactCount = 0;
    bool IsGrounded => groundContactCount > 0;

    float jumpInputLockTime = 0f;
    const float JUMP_LOCK_DURATION = 0.15f;

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        mainCam = p2Camera != null
            ? p2Camera.transform
            : Camera.main.transform;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping  = 0f;
        rb.angularDamping = 0.05f;
        animator.applyRootMotion = false;

        if (cinemachineFreeLook != null)
        {
            var go = new GameObject("CameraTarget_P2");
            var t  = go.transform;
            t.SetParent(transform);
            t.localPosition = new Vector3(0f, 1.5f, 0f);
            cinemachineFreeLook.Target.TrackingTarget     = t;
            cinemachineFreeLook.Target.CustomLookAtTarget = false;
            cinemachineInput = cinemachineFreeLook
                .GetComponent<CinemachineInputAxisController>();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value    = currentHealth;
        }
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // GROUNDING
    // ─────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────
    // UPDATE — INPUT (Arrow Keys + Period + Slash)
    // ─────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (isDead) return;

        // Tick down jump lock timer
        jumpInputLockTime -= Time.deltaTime;
        bool inputLocked = jumpInputLockTime > 0f;

        var kb = Keyboard.current;
        float x = 0f, z = 0f;

        if (kb.upArrowKey.isPressed)    z =  1f;
        if (kb.downArrowKey.isPressed)  z = -1f;
        if (kb.leftArrowKey.isPressed)  x = -1f;
        if (kb.rightArrowKey.isPressed) x =  1f;

        if (kb.periodKey.wasPressedThisFrame && IsGrounded) DoJump();
        if (kb.slashKey.wasPressedThisFrame)                DoAttack();

        // Movement direction — locked for 0.15s after jump
        bool hasInput = !inputLocked && (x != 0f || z != 0f);

        if (hasInput)
        {
            var camF = mainCam.forward; camF.y = 0f; camF.Normalize();
            var camR = mainCam.right;   camR.y = 0f; camR.Normalize();
            moveDir  = (camF * z + camR * x).normalized;
        }
        else if (!inputLocked)
        {
            moveDir = Vector3.zero;
        }
        // If inputLocked: keep existing moveDir (carries jump direction)

        // Rotation
        if (moveDir.sqrMagnitude > 0.01f)
        {
            var targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot,
                rotationSpeed * Time.deltaTime);
        }

        // Animator speed
        float targetSpeed = hasInput ? animWalkSpeed : 0f;
        animator.SetFloat(Speed,
            Mathf.MoveTowards(
                animator.GetFloat(Speed),
                targetSpeed,
                Time.deltaTime * 10f));
    }

    // ─────────────────────────────────────────────────────────────────────
    // FIXED UPDATE — PHYSICS
    // ─────────────────────────────────────────────────────────────────────

    void FixedUpdate()
    {
        if (isDead) return;

        if (IsGrounded)
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 slopeMove = ProjectOnSlope(moveDir);
                Vector3 velocity  = new Vector3(
                    slopeMove.x * moveSpeed,
                    rb.linearVelocity.y,
                    slopeMove.z * moveSpeed);
                Vector3 platformVel = GetPlatformVelocity();
platformVel.y = 0f;
velocity += platformVel * 0.4f;
                rb.linearVelocity = velocity;
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    0f, rb.linearVelocity.y, 0f);
            }
        }
        else
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 airForce = new Vector3(
                    moveDir.x * moveSpeed * airControl,
                    0f,
                    moveDir.z * moveSpeed * airControl);
                rb.AddForce(airForce, ForceMode.VelocityChange);

                Vector3 horizontalVel = new Vector3(
                    rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (horizontalVel.magnitude > moveSpeed)
                {
                    Vector3 clamped = horizontalVel.normalized * moveSpeed;
                    rb.linearVelocity = new Vector3(
                        clamped.x, rb.linearVelocity.y, clamped.z);
                }
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x * airDamping,
                    rb.linearVelocity.y,
                    rb.linearVelocity.z * airDamping);
            }

            rb.AddForce(Vector3.down * 8f, ForceMode.Acceleration);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ACTIONS
    // ─────────────────────────────────────────────────────────────────────

    void DoJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        groundContactCount = 0;
        animator.SetBool(IsJumping, true);
        jumpInputLockTime = JUMP_LOCK_DURATION;
    }

    void DoAttack()
    {
        animator.SetTrigger(AttackHash);
        AttackNearbyEnemies();
    }

    void AttackNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null) { enemy.TakeDamage(1); continue; }
            var boss = hit.GetComponent<BossGuardian>();
            if (boss != null) boss.TakeDamage(1);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // HEALTH
    // ─────────────────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (currentHealth > 0)
            animator.SetTrigger(GetHitHash);
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) StartCoroutine(DieSequence());
    }

    IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var original  = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            original[i] = renderers[i].material.GetColor(BaseColorID);
        foreach (var r in renderers)
            r.material.SetColor(BaseColorID, Color.red);
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(BaseColorID, original[i]);
    }

    IEnumerator DieSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        animator.SetTrigger(DieHash);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (cinemachineInput != null)
            cinemachineInput.enabled = false;

        yield return new WaitForSeconds(1.5f);
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────────────
    // RESPAWN / QUIT
    // ─────────────────────────────────────────────────────────────────────

    public void Respawn()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        StopAllCoroutines();

        isDead             = false;
        currentHealth      = maxHealth;
        groundContactCount = 0;
        moveDir            = Vector3.zero;
        jumpInputLockTime  = 0f;

        if (healthSlider != null) healthSlider.value = currentHealth;

        transform.position = respawnPoint != null
            ? respawnPoint.position
            : new Vector3(2f, 1f, 0f);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic    = false;

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
        Cursor.visible   = false;
        if (cinemachineInput != null)
            cinemachineInput.enabled = true;
    }

    public void QuitGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    Vector3 ProjectOnSlope(Vector3 direction)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
            Vector3.down, out RaycastHit hit, 0.4f))
            return Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        return direction;
    }

    Vector3 GetPlatformVelocity()
    {
        if (Physics.Raycast(transform.position, Vector3.down,
            out RaycastHit hit, 1.5f))
        {
            var platform = hit.collider.GetComponent<PlatformMover>();
            if (platform != null) return platform.Velocity;
        }
        return Vector3.zero;
    }
}