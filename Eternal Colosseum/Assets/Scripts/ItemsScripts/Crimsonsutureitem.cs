using UnityEngine;

[CreateAssetMenu(fileName = "CrimsonSuture", menuName = "Inventory/Coliseum Items/Crimson Suture")]
public class CrimsonSutureItem : ItemData
{
    [Header("Lifesteal Settings")]
    public float healPerHit = 1f;

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.Heal(healPerHit);
        Debug.Log($"[ITEM] Crimson Suture pulled — healed {healPerHit} HP on hit.");
    }
}