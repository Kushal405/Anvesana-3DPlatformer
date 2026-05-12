// using UnityEngine;

// public class WinTrigger : MonoBehaviour
// {
//     void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Player"))
//             RaceManager.Instance?.PlayerWon(1);
//         else if (other.CompareTag("Player2"))
//             RaceManager.Instance?.PlayerWon(2);
//     }
// }
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("WIN TRIGGER HIT BY: " + other.name);
        Debug.Log("TAG: " + other.tag);

        Transform root = other.transform.root;

        Debug.Log("ROOT: " + root.name);
        Debug.Log("ROOT TAG: " + root.tag);

        if (other.CompareTag("Player") || root.CompareTag("Player"))
        {
            Debug.Log("PLAYER 1 WON");
            RaceManager.Instance?.PlayerWon(1);
        }

        if (other.CompareTag("Player2") || root.CompareTag("Player2"))
        {
            Debug.Log("PLAYER 2 WON");
            RaceManager.Instance?.PlayerWon(2);
        }
    }
}