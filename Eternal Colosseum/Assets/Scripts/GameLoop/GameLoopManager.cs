using UnityEngine;
using UnityEngine.SceneManagement;

// Handles core game flow: arena, shop, progression, and end-game states.
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;
    public WaveClearUI waveClearUI;
    public VictoryUI victoryUI;

    public int currentLevel = 1;
    public int currentStage = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnWaveCompleted()
    {
        Debug.Log("[GAME LOOP] Wave Complete!");

        if (currentLevel >= 16 && currentStage >= 4)
        {
            victoryUI.ShowVictory();
            return;
        }

        waveClearUI.ShowWaveClear();
    }

    public void LoadShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void AdvanceStage()
    {
        currentStage++;

        // Progress through 4 stages before advancing to the next level.
        if (currentStage > 4)
        {
            currentStage = 1;
            currentLevel++;
        }

        Debug.Log($"[GAME LOOP] Level {currentLevel} Stage {currentStage}");
    }
    
    public void LoadArena()
    {
        SceneManager.LoadScene("ArenaScene");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[GAME LOOP] Test shop transition");
            LoadShop();
        }
    }
}