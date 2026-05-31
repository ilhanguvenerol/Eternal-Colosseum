using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    public GameObject victoryPanel;

    // Display the victory screen and pause gameplay.
    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}