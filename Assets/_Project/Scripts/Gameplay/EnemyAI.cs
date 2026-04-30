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
    Animator animator;
    NavMeshAgent agent;
    bool isAttacking = false;
    float attackAnimDuration = 0.8f;
    Transform currentTarget;

    void Start()
    {
        currentTarget = pointB;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Fix sinking — let NavMeshAgent control Y position
        agent.updatePosition = true;
        agent.updateRotation = true;
        

        // Disable Rigidbody gravity — NavMeshAgent handles grounding
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // NavMeshAgent takes full control
        }

        currentHealth = maxHealth;


    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (isAttacking) return;

        if (dist <= attackRange)
        {
            // In attack range — stop and attack
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
    if (currentTarget == null) return;

    // Set stopping distance LOW for patrol
    agent.stoppingDistance = 0.2f;
    agent.speed = patrolSpeed;
    agent.SetDestination(currentTarget.position);
    animator?.SetFloat("Speed", patrolSpeed);

    if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
        currentTarget = currentTarget == pointA ? pointB : pointA;
}

void ChasePlayer()
{
    // Restore attack stopping distance when chasing
    agent.stoppingDistance = attackRange - 0.3f;
    agent.speed = chaseSpeed;
    agent.SetDestination(player.position);
    animator?.SetFloat("Speed", chaseSpeed);
}
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (isAttacking) return;

        // Face player before attacking
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            lastAttackTime = Time.time;
            pc.TakeDamage(damage);
            animator?.SetFloat("Speed", 0f);
            animator?.SetTrigger("Attack");
            StartCoroutine(AttackRoutine());
        }
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
        Debug.Log($"Enemy took damage! Health: {currentHealth}/{maxHealth}");
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) Die();
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

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}