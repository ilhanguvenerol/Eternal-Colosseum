using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    public GameObject victoryPanel;

    private void Awake()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.victoryUI = this;
        }
    }

    // Display the victory screen and pause gameplay.
    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}