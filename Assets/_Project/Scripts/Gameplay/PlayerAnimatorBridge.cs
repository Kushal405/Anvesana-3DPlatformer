using UnityEngine;

public class PlayerAnimatorBridge : MonoBehaviour
{
    const string IDLE    = "Idle_Battle_SwordAndShield";
    const string WALK    = "MoveFWD_Battle_SwordAndShield";
    const string SPRINT  = "SprintFWD_Battle_SwordAndShield";
    const string JUMP    = "JumpFull_Normal_SwordAndShield";
    const string ATTACK  = "Attack01_SwordAndShield";
    const string GET_HIT = "GetHit01_SwordAndShield";
    const string DIE     = "Die01_SwordAndShield";
    const string GET_UP  = "GetUp_SwordAndShield";

    Animator animator;

    // Track state without blocking transitions
    string currentState = "";
    float stateTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (stateTimer > 0f)
            stateTimer -= Time.deltaTime;
    }

    void PlayState(string stateName, float crossFade = 0.15f, float minDuration = 0f)
    {
        // Only block if same state AND within minimum duration window
        if (currentState == stateName && stateTimer > 0f) return;

        currentState = stateName;
        stateTimer = minDuration;

        Debug.Log($"CrossFade → {stateName}");
        animator.CrossFadeInFixedTime(stateName, crossFade, 0);
    }

    // ── Public API ─────────────────────────────────────────────
    public void PlayIdle()   => PlayState(IDLE,    0.2f, 0f);
    public void PlayWalk()   => PlayState(WALK,    0.15f, 0f);
    public void PlaySprint() => PlayState(SPRINT,  0.15f, 0f);
    public void PlayJump()   => PlayState(JUMP,    0.05f, 0.5f); // lock for 0.5s
    public void PlayAttack() => PlayState(ATTACK,  0.05f, 0.8f); // lock for 0.8s
    public void PlayGetHit() => PlayState(GET_HIT, 0.05f, 0.3f);
    public void PlayDie()    => PlayState(DIE,     0.1f,  2.5f);
    public void PlayGetUp()  => PlayState(GET_UP,  0.1f,  1f);

    public void UpdateLocomotion(bool hasInput, bool isGrounded, bool isSprinting)
    {
        // Don't override locked states (attack, jump, die etc.)
        if (stateTimer > 0f) return;

        if (!isGrounded)
        {
            PlayState(JUMP, 0.1f, 0f);
            return;
        }

        if (!hasInput)
        {
            PlayIdle();
            return;
        }

        if (isSprinting)
            PlaySprint();
        else
            PlayWalk();
    }
}