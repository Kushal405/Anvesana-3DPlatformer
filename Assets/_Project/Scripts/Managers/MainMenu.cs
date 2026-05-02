using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void PlaySinglePlayer()
    {
        GameModeManager.CurrentMode = 
            GameModeManager.GameMode.SinglePlayer;
        SceneManager.LoadScene("SampleScene");
    }

    public void PlayMultiPlayer()
    {
        GameModeManager.CurrentMode = 
            GameModeManager.GameMode.MultiPlayer;
        SceneManager.LoadScene("SampleScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}