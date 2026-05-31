using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    public GameObject victoryPanel;

    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}