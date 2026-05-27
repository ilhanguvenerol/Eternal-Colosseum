using UnityEngine;

[CreateAssetMenu(fileName = "ExecutionersVerdict", menuName = "Inventory/Coliseum Items/Executioner's Verdict")]
public class ExecutionersVerdictItem : ItemData
{
    [Header("Verdict Settings")]
    public float damageMultiplier = 1.5f; // +50% damage on full-health targets

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        EnemyHealth enemyHP = targetEnemy.GetComponentInParent<EnemyHealth>();
        if (enemyHP == null) return;

        if (enemyHP.CurrentHealth >= enemyHP.MaxHealth)
        {
            float bonusDamage = damageDealt * (damageMultiplier - 1f);
            enemyHP.TakeDamage(bonusDamage, null);
            Debug.Log($"[ITEM] Executioner's Verdict struck! {bonusDamage} bonus damage on unwounded {targetEnemy.name}.");
        }
    }
}