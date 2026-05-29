using UnityEngine;

[CreateAssetMenu(fileName = "LastRiteMedallion", menuName = "Inventory/Coliseum Items/Last Rite Medallion")]
public class LastRiteMedallionItem : ItemData
{
    [Header("Revival Settings")]
    public float reviveHealthPercent = 0.5f; // Revives to 50% of max HP

    // Runtime flag — resets if the item is removed and re-added
    private bool _consumed = false;

    public override float OnTakeDamageEffect(GameObject player, float damageTaken)
    {
        if (_consumed) return damageTaken;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        if (hp == null) return damageTaken;

        if (hp.CurrentHealth - damageTaken <= 0f)
        {
            _consumed = true;
            Inventory.Instance?.ownedItems.Remove(this); // Shatter the medallion
            hp.Heal(hp.MaxHealth * reviveHealthPercent);  // Rise
            Debug.Log("[ITEM] Last Rite Medallion shattered. Death refused.");
            return 0f; // Block the killing blow
        }

        return damageTaken;
    }
}