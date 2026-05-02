using UnityEngine;

public class MobileControlsCanvas : MonoBehaviour
{
    void Start()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
            gameObject.SetActive(false);
        #endif
    }
}