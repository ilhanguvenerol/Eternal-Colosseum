using UnityEngine;

[CreateAssetMenu(fileName = "SurgeonsVial", menuName = "Inventory/Coliseum Items/Surgeon's Vial")]
public class SurgeonsVialItem : ItemData
{
    [Header("Vial Settings")]
    public float healAmount = 20f;
    public float healDelay = 2f; // Seconds before the heal kicks in

    public override float OnTakeDamageEffect(GameObject player, float damageTaken)
    {

        player.GetComponent<PlayerHealth>()?.HealAfterDelay(healAmount, healDelay);
        Debug.Log($"[ITEM] Surgeon's Vial smashed — healing {healAmount} HP in {healDelay}s.");
        return damageTaken; // Does not reduce incoming damage
    }
}