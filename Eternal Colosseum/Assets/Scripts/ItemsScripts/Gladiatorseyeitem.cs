using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorsEye", menuName = "Inventory/Coliseum Items/Gladiator's Eye")]
public class GladiatorsEyeItem : ItemData
{
    [Header("Focus Settings")]
    public float closeRangeThreshold = 3f;
    public float bonusMultiplier = 1.2f; // +20% damage up close

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        float distance = Vector3.Distance(player.transform.position, targetEnemy.transform.position);
        if (distance > closeRangeThreshold) return;

        float extraDamage = damageDealt * (bonusMultiplier - 1f);
        EnemyHealth enemy = targetEnemy.GetComponentInParent<EnemyHealth>();
        enemy?.TakeDamage(extraDamage, null);

        Debug.Log($"[ITEM] Gladiator's Eye focused — {extraDamage} bonus close-range damage.");
    }
}