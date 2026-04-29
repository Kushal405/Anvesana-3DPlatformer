using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;  // Added for IEnumerator

public class PlayerController : ValidatedMonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    int currentHealth;
    public UnityEngine.UI.Slider healthSlider;

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

void Start()
{
    currentHealth = maxHealth;
    
    if (healthSlider != null)
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        Debug.Log($"✓ Health slider found and initialized: {currentHealth}/{maxHealth}");
    }
    else
    {
        Debug.LogError("✗ Health slider is NULL! Drag PlayerHealthBar into the field!");
    }
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
        // TEMPORARY: Add this inside Update() for testing
if (Keyboard.current.tKey.wasPressedThisFrame)
{
    TakeDamage(1);
    Debug.Log("Damage taken! Health: " + currentHealth);
}
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

   public void TakeDamage(int amount)
{
    currentHealth -= amount;
    Debug.Log($"Damage taken! Current health: {currentHealth}");
    
    if (healthSlider != null)
    {
        healthSlider.value = currentHealth;
        Debug.Log($"Slider value set to: {healthSlider.value}");
    }
    else
    {
        Debug.LogError("Health slider is NULL!");
    }

    StartCoroutine(DamageFlash());

    if (currentHealth <= 0)
        Die();
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
        // Respawn at spawn point (adjust coordinates as needed)
        transform.position = new Vector3(0f, 1f, 0f);
        currentHealth = maxHealth;
        if (healthSlider != null)
            healthSlider.value = currentHealth;
        
        // Optional: Add respawn effects or sound here
    }
}