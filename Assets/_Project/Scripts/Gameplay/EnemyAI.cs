using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] float patrolSpeed = 2f;

    [Header("Chase")]
    [SerializeField] float chaseRange = 5f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("Attack")]
[SerializeField] int damage = 1;
[SerializeField] float attackCooldown = 1f;
float lastDamageTime = 0f;

    Transform currentTarget;
    Transform player;
    Animator animator;
    bool waiting = false;

    void Start()
    {
        currentTarget = pointB;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= chaseRange)
            ChasePlayer();
        else
            Patrol();

        // Face movement direction
        Vector3 target = dist <= chaseRange ? 
            player.position : currentTarget.position;
        FaceTarget(target);
    }

    void Patrol()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            patrolSpeed * Time.deltaTime);

        // Animate walking
        if (animator != null)
            animator.SetFloat("Speed", patrolSpeed);

        if (Vector3.Distance(
            transform.position, currentTarget.position) < 0.2f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime);

        // Animate running
        if (animator != null)
            animator.SetFloat("Speed", chaseSpeed);
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude > 0.1f)
        {
            var rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    void OnTriggerStay(Collider col)
{
    if (!col.CompareTag("Player")) return;
    
    // Only damage once per cooldown
    if (Time.time - lastDamageTime < attackCooldown)
        return;

    PlayerController pc = col.GetComponent<PlayerController>();
    if (pc != null)
    {
        pc.TakeDamage(damage);
        lastDamageTime = Time.time;
        Debug.Log("Enemy hit player!");
    }
}
}