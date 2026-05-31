using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

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

        LoadShop();
    }

    public void LoadShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void AdvanceStage()
    {
        currentStage++;

        if (currentStage > 3)
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