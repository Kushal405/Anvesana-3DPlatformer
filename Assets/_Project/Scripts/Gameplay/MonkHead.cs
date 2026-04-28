using DG.Tweening;
using UnityEngine;

public class MonkHead : MonoBehaviour
{
    [SerializeField] SacredGate sacredGate;
    [SerializeField] ParticleSystem awakeningParticles;

    [Header("Settings")]
    [SerializeField] Vector3 flatRotation = new Vector3(-90f, -180f, 0f);
    [SerializeField] Vector3 uprightRotation = new Vector3(0f, -180f, 0f);
    [SerializeField] float riseHeight = 2f;
    [SerializeField] float riseDuration = 2f;

    bool awakened = false;

    void Start()
    {
        transform.rotation = Quaternion.Euler(flatRotation);
    }

    public void Awaken()
    {
        if (awakened) return;
        awakened = true;

        Debug.Log("Awaken called!");

        // Safe particle play
        try { awakeningParticles?.Play(); } 
        catch { Debug.Log("No particles assigned - skipping"); }

        transform.DOMove(
            transform.position + Vector3.up * riseHeight,
            riseDuration)
            .SetEase(Ease.OutBounce);

        transform.DORotate(
            uprightRotation,
            riseDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Debug.Log("Rising complete - opening gate");
                sacredGate?.OpenGate();
            });
    }
}