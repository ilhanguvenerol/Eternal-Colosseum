using UnityEngine;

public class ShopContinueButton : MonoBehaviour
{
    public void ContinueGame()
    {
        Debug.Log("[GAME LOOP] Continue pressed");

        GameLoopManager.Instance.LoadArena();
    }
}