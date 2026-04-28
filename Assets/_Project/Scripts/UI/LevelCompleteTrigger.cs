using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI completeText;
    [SerializeField] GameObject completPanel;
    bool completed = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (completed) return;
        completed = true;
        completPanel.SetActive(true);
        completeText.text = "You Reached Swayambhu!\nnLevel Complete!";
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}