using UnityEngine;

[CreateAssetMenu(fileName = "ConquerorsBrand", menuName = "Inventory/Coliseum Items/Conqueror's Brand")]
public class ConquerorsBrandItem : ItemData
{
    [Header("Tempering Settings")]
    public float healthGainPerKill = 1f;

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.MaxHealth += healthGainPerKill;
        health.Heal(healthGainPerKill); // Grant the HP instantly
        Debug.Log($"[ITEM] Conqueror's Brand tempered. Permanent Max HP: {health.MaxHealth}");
    }
}