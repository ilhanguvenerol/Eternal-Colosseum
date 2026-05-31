using UnityEngine;

public class ShopContinueButton : MonoBehaviour
{
    // Called when the player clicks the "Continue" button in the shop.
    public void ContinueGame()
    {
        Debug.Log("[GAME LOOP] Continue pressed");

        GameLoopManager.Instance.AdvanceStage();
        GameLoopManager.Instance.LoadArena();
    }
}