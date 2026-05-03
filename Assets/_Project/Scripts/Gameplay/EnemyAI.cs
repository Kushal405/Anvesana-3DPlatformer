using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] float patrolSpeed = 2f;

    [Header("Chase")]
    [SerializeField] float chaseRange = 8f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("Attack")]
    [SerializeField] int damage = 1;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float attackRange = 1.8f;
    float lastAttackTime = -999f;

    [Header("Health")]
    [SerializeField] int maxHealth = 3;
    int currentHealth;

    Transform player;
    Transform player2;
    Transform currentChaseTarget;

    Animator animator;
    NavMeshAgent agent;
    bool isAttacking = false;
    float attackAnimDuration = 0.8f;
    Transform patrolTarget;

    void Start()
    {
        patrolTarget = pointB;

        var p1GO = GameObject.FindGameObjectWithTag("Player");
        var p2GO = GameObject.FindGameObjectWithTag("Player2");
        if (p1GO != null) player = p1GO.transform;
        if (p2GO != null) player2 = p2GO.transform;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        currentHealth = maxHealth;
    }

    Transform GetClosestPlayer()
    {
        if (player == null && player2 == null) return null;
        if (player == null) return player2;
        if (player2 == null) return player;

        float d1 = Vector3.Distance(transform.position, player.position);
        float d2 = Vector3.Distance(transform.position, player2.position);
        return d1 < d2 ? player : player2;
    }

    void Update()
    {
        currentChaseTarget = GetClosestPlayer();
        if (currentChaseTarget == null) return;

        float dist = Vector3.Distance(
            transform.position, currentChaseTarget.position);

        if (isAttacking) return;

        if (dist <= attackRange)
        {
            agent.ResetPath();
            animator?.SetFloat("Speed", 0f);
            TryAttack();
        }
        else if (dist <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolTarget == null) return;
        agent.stoppingDistance = 0.2f;
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolTarget.position);
        animator?.SetFloat("Speed", patrolSpeed);

        if (Vector3.Distance(
            transform.position, patrolTarget.position) < 0.5f)
            patrolTarget = patrolTarget == pointA ? pointB : pointA;
    }

    void ChasePlayer()
    {
        agent.stoppingDistance = attackRange - 0.3f;
        agent.speed = chaseSpeed;
        agent.SetDestination(currentChaseTarget.position);
        animator?.SetFloat("Speed", chaseSpeed);
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (isAttacking) return;

        Vector3 dir = (currentChaseTarget.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        PlayerController pc = currentChaseTarget
            .GetComponent<PlayerController>();
        P2PlayerController pc2 = currentChaseTarget
            .GetComponent<P2PlayerController>();

        if (pc != null)
        {
            lastAttackTime = Time.time;
            pc.TakeDamage(damage);
            PlayAttackAnim();
        }
        else if (pc2 != null)
        {
            lastAttackTime = Time.time;
            pc2.TakeDamage(damage);
            PlayAttackAnim();
        }
    }

    void PlayAttackAnim()
    {
        animator?.SetFloat("Speed", 0f);
        animator?.SetTrigger("Attack");
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.enabled = false;
        yield return new WaitForSeconds(attackAnimDuration);
        agent.enabled = true;
        isAttacking = false;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy took damage! HP: {currentHealth}/{maxHealth}");
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material
                .GetColor(Shader.PropertyToID("_BaseColor"));
        foreach (var r in renderers)
            r.material.SetColor(
                Shader.PropertyToID("_BaseColor"), Color.red);
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(
                Shader.PropertyToID("_BaseColor"), originalColors[i]);
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}