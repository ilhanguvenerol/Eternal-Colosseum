using UnityEngine;

[CreateAssetMenu(fileName = "WarlordsBrand", menuName = "Inventory/Coliseum Items/Warlord's Brand")]
public class WarlordsBrandItem : ItemData
{
    [Header("Shockwave Settings")]
    public float shockwaveRadius = 3f;
    public float damagePercentage = 0.6f; // 60% of original hit as AOE

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        Debug.Log("[ITEM] Warlord's Brand shockwave!");

        float aoeDamage = damageDealt * damagePercentage;

        Collider[] nearby = Physics.OverlapSphere(targetEnemy.transform.position, shockwaveRadius);
        foreach (Collider col in nearby)
        {
            if (col.gameObject == targetEnemy) continue; // Skip the primary target

            EnemyHealth enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(aoeDamage, null);
        }
    }
}