using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
    public GameObject boulderPrefab;
    public float spawnInterval = 3f;
    public float boulderSpeed = 6f;
    public Vector3 rollDirection = new Vector3(-1, -0.3f, 0);
    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnBoulder();
            timer = spawnInterval;
        }
    }

    void SpawnBoulder()
    {
        GameObject b = Instantiate(boulderPrefab, transform.position, Random.rotation);
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = rollDirection.normalized * boulderSpeed;
        Destroy(b, 8f); 
    }
}