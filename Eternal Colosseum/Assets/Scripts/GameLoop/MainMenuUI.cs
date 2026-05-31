using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // Load the arena scene when the player clicks the "Start Game" button.
    public void StartGame()
    {
        SceneManager.LoadScene("ArenaScene");
    }
}