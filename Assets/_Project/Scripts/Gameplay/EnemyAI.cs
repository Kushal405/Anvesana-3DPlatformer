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
    Transform patrolTarget;

    Animator animator;
    NavMeshAgent agent;
    bool isAttacking = false;
    float attackAnimDuration = 0.8f;

    void Start()
    {
        patrolTarget = pointB;

        var p1GO = GameObject.FindGameObjectWithTag("Player");
        var p2GO = GameObject.FindGameObjectWithTag("Player2");
        if (p1GO != null) player = p1GO.transform;
        if (p2GO != null) player2 = p2GO.transform;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        currentHealth = maxHealth;
    }

    // SAFE CHECK
    bool AgentReady()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    Transform GetClosestPlayer()
    {
        bool p1Dead = player != null &&
            player.GetComponent<PlayerController>()?.IsDead == true;
        bool p2Dead = player2 != null &&
            player2.GetComponent<P2PlayerController>()?.IsDead == true;

        Transform alive1 = p1Dead ? null : player;
        Transform alive2 = p2Dead ? null : player2;

        if (alive1 == null && alive2 == null) return null;
        if (alive1 == null) return alive2;
        if (alive2 == null) return alive1;

        float d1 = Vector3.Distance(transform.position, alive1.position);
        float d2 = Vector3.Distance(transform.position, alive2.position);
        return d1 < d2 ? alive1 : alive2;
    }

    void Update()
    {
        currentChaseTarget = GetClosestPlayer();

        if (currentChaseTarget == null)
        {
            Patrol();
            return;
        }

        float dist = Vector3.Distance(
            transform.position, currentChaseTarget.position);

        if (isAttacking) return;

        if (dist <= attackRange)
        {
            if (AgentReady()) agent.ResetPath();
            animator?.SetFloat("Speed", 0f);
            TryAttack();
        }
        else if (dist <= chaseRange)
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        if (patrolTarget == null) return;
        if (!AgentReady()) return;

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
        if (!AgentReady()) return;

        agent.stoppingDistance = attackRange - 0.3f;
        agent.speed = chaseSpeed;
        agent.SetDestination(currentChaseTarget.position);

        animator?.SetFloat("Speed", chaseSpeed);
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (isAttacking) return;

        float dist = Vector3.Distance(
            transform.position, currentChaseTarget.position);
        if (dist > attackRange + 0.5f) return;

        PlayerController pc = currentChaseTarget
            .GetComponent<PlayerController>();
        P2PlayerController pc2 = currentChaseTarget
            .GetComponent<P2PlayerController>();

        if (pc != null && pc.IsDead) return;
        if (pc2 != null && pc2.IsDead) return;

        Vector3 dir = currentChaseTarget.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

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

        if (agent != null) agent.enabled = false;

        yield return new WaitForSeconds(attackAnimDuration);

        if (agent != null && agent.isActiveAndEnabled)
            agent.enabled = true;

        isAttacking = false;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy HP: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var origColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            origColors[i] = renderers[i].material
                .GetColor(Shader.PropertyToID("_BaseColor"));

        foreach (var r in renderers)
            r.material.SetColor(
                Shader.PropertyToID("_BaseColor"), Color.red);

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.SetColor(
                Shader.PropertyToID("_BaseColor"), origColors[i]);
    }

    void Die()
    {
        AudioManager.Instance?.PlayEnemyDeath();
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