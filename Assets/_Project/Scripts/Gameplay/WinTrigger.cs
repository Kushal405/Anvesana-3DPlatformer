using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            RaceManager.Instance?.PlayerWon(1);
        else if (other.CompareTag("Player2"))
            RaceManager.Instance?.PlayerWon(2);
    }
}