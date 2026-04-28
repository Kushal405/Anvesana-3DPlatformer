using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] float parallaxStrength = 0.1f;

    Vector3 startPosition;
    float startZ;

    void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        float distanceFromPlayer = playerTransform.position.z - startZ;
        float parallaxAmount = distanceFromPlayer * parallaxStrength;
        transform.position = new Vector3(
            startPosition.x + playerTransform.position.x * parallaxStrength,
            startPosition.y,
            startPosition.z
        );
    }
}