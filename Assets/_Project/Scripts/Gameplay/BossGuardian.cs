using UnityEngine;
using System.Collections;

public class BossGuardian : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    int currentHealth;

    [Header("Movement")]
    public Transform patrolA;
    public Transform patrolB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public float chaseRange = 8f;
    public float attackRange = 1.8f;
    public float attackCooldown = 2f;
    public float rotationSpeed = 10f;

    [Header("UI")]
    public UnityEngine.UI.Slider bossHealthBar;
    public GameObject bossHealthUI;

    [Header("Invincibility")]
    public float invincibilityDuration = 0.5f;

    // ── Same parameters as player animator ──────────────────────
    static readonly int Speed     = Animator.StringToHash("Speed");
    static readonly int IsJumping = Animator.StringToHash("IsJumping");
    static readonly int AttackHash  = Animator.StringToHash("Attack");
    static readonly int GetHitHash  = Animator.StringToHash("GetHit");
    static readonly int DieHash     = Animator.StringToHash("Die");

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    Rigidbody rb;
    Animator animator;
    Transform player;
    PlayerController playerController;
    Transform patrolTarget;

    float attackTimer;
    bool isDead;
    bool isInvincible;
    bool isAttacking;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping = 2f;

        SetBossBlack();
    }

    void Start()
    {
        currentHealth = maxHealth;
        patrolTarget = patrolA;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            playerController = playerGO.GetComponent<PlayerController>();
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.minValue = 0;
            bossHealthBar.maxValue = maxHealth;
            bossHealthBar.value = maxHealth;
        }
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (bossHealthUI != null)
            bossHealthUI.SetActive(dist < chaseRange + 4f);

        attackTimer -= Time.deltaTime;

        if (dist <= chaseRange)
            ChasePlayer(dist);
        else
            Patrol();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isAttacking)
        {
            Vector3 moveDir = rb.linearVelocity;
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(
                    transform.rotation, targetRot,
                    rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }

    void Patrol()
    {
        if (isAttacking) return;

        Vector3 dir = patrolTarget.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (dist > 0.3f)
        {
            // Use Speed parameter — same as player
            animator.SetFloat(Speed, 0.5f);
            rb.linearVelocity = new Vector3(
                dir.normalized.x * patrolSpeed,
                rb.linearVelocity.y,
                dir.normalized.z * patrolSpeed);
        }
        else
        {
            animator.SetFloat(Speed, 0f);
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            patrolTarget = patrolTarget == patrolA ? patrolB : patrolA;
        }
    }

    void ChasePlayer(float dist)
    {
        if (dist <= attackRange)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            animator.SetFloat(Speed, 0f);

            if (attackTimer <= 0f && !isAttacking)
            {
                StartCoroutine(DoAttack());
                attackTimer = attackCooldown;
            }
        }
        else
        {
            if (isAttacking) return;

            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            dir.Normalize();

            bool sprinting = dist > chaseRange * 0.5f;
            float speed = sprinting ? chaseSpeed : patrolSpeed;

            // Send 1.0 for sprint, 0.5 for walk — matches blend tree
            animator.SetFloat(Speed, sprinting ? 1f : 0.5f);

            rb.linearVelocity = new Vector3(
                dir.x * speed,
                rb.linearVelocity.y,
                dir.z * speed);
        }
    }

    IEnumerator DoAttack()
    {
        isAttacking = true;
        animator.SetFloat(Speed, 0f);
        animator.SetTrigger(AttackHash);
        

        yield return new WaitForSeconds(0.45f);

        // Deal damage
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 knockback = (player.position - transform.position).normalized;
            knockback.y = 0.3f;
            playerRb.AddForce(knockback * 5f, ForceMode.Impulse);
        }
        playerController?.TakeDamage(1);
        

        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isInvincible) return;
        

        currentHealth -= amount;
        if (bossHealthBar != null)
            bossHealthBar.value = currentHealth;
            AudioManager.Instance?.PlayBossHurt();

        animator.SetTrigger(GetHitHash);
        StartCoroutine(HitFlash());
        StartCoroutine(InvincibilityWindow());

        if (currentHealth <= 0)
            StartCoroutine(DieSequence());
    }

    IEnumerator HitFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var origColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            origColors[i] = renderers[i].material.GetColor(BaseColor);
        foreach (var r in renderers)
            r.material.SetColor(BaseColor, Color.white);
        yield return new WaitForSeconds(0.15f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(BaseColor, origColors[i]);
    }

    IEnumerator InvincibilityWindow()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    IEnumerator DieSequence()
    {
        isDead = true;
        AudioManager.Instance?.PlayBossDeath();
        isAttacking = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        animator.SetTrigger(DieHash);

        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);

        yield return new WaitForSeconds(2f);
        GameManager.Instance?.OnBossDefeated();
        Destroy(gameObject);
    }

    void SetBossBlack()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(mats[i]);
                mats[i].SetColor(BaseColor, Color.black);
                mats[i].SetColor("_EmissionColor", new Color(0.04f, 0.04f, 0.04f));
            }
            r.materials = mats;
        }
    }

    void LateUpdate()
    {
        if (bossHealthUI == null) return;
        bossHealthUI.transform.LookAt(
            bossHealthUI.transform.position +
            Camera.main.transform.rotation * Vector3.forward);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}