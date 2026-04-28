using DG.Tweening;
using UnityEngine;

public class SacredGate : MonoBehaviour
{
    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;

    void Start()
    {
        if (leftDoor) leftDoor.SetActive(true);
        if (rightDoor) rightDoor.SetActive(true);
    }

    public void OpenGate()
    {
        if (leftDoor)
            leftDoor.transform.DORotate(
                new Vector3(0f, -90f, 0f), 1.5f)
                .SetEase(Ease.OutBack);

        if (rightDoor)
            rightDoor.transform.DORotate(
                new Vector3(0f, 90f, 0f), 1.5f)
                .SetEase(Ease.OutBack);
    }
}