using UnityEngine;

[CreateAssetMenu(fileName = "FallenChampionsTrophy", menuName = "Inventory/Coliseum Items/Fallen Champion's Trophy")]
public class FallenChampionsTrophyItem : ItemData
{
    [Header("Healing Settings")]
    public float healAmount = 8f;

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        Debug.Log("[ITEM] Fallen Champion's Trophy — instantly healing player!");

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.Heal(healAmount);
        }
    }
}