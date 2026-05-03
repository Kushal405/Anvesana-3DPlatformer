using DG.Tweening;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    [SerializeField] Vector3 moveTo = new Vector3(3f, 0f, 0f);
    [SerializeField] float duration = 2f;
    [SerializeField] Ease easeMethod = Ease.InOutSine;

    Vector3 startPosition;
    Vector3 lastPosition;

    public Vector3 Velocity { get; private set; }

    void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;
        Move();
    }

    void FixedUpdate()
    {
        Velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    void Move()
    {
        transform.DOMove(startPosition + moveTo, duration)
            .SetEase(easeMethod)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }
}