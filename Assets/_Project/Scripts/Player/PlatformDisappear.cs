using UnityEngine;
using DG.Tweening;

public class DisappearPlatform : MonoBehaviour
{
    public float waitTime = 1.5f;
    public float hideTime = 2f;
    Renderer rend;
    Collider col;
    bool triggered = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        col  = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (!triggered && 
        (c.gameObject.CompareTag("Player")||
             c.gameObject.CompareTag("Player2")))
        {
            triggered = true;
            StartCoroutine(DisappearRoutine());
        }
    }

    System.Collections.IEnumerator DisappearRoutine()
    {
        rend.material.DOColor(Color.red, waitTime * 0.5f);
        yield return new WaitForSeconds(waitTime);

        col.enabled = false;
        rend.enabled = false;
        yield return new WaitForSeconds(hideTime);

        col.enabled = true;
        rend.enabled = true;
        rend.material.DOColor(Color.white, 0.3f);
        triggered = false;
    }
}