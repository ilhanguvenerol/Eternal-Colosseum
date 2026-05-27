using UnityEngine;

[CreateAssetMenu(fileName = "ThunderstoneShard", menuName = "Inventory/Coliseum Items/Thunderstone Shard")]
public class ThunderstoneShardItem : ItemData
{
    [Header("Detonation Settings")]
    [Range(0f, 1f)] public float procChance = 0.10f; // 10% chance per hit
    public float explosionRadius = 4f;
    public float explosionDamage = 35f;
    public GameObject explosionEffectPrefab; // Assign a shockwave VFX in the Inspector

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        if (Random.value > procChance) return;

        Debug.Log("[ITEM] Thunderstone Shard detonated!");

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, targetEnemy.transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(targetEnemy.transform.position, explosionRadius);
        foreach (Collider col in hits)
        {
            EnemyHealth enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(explosionDamage, null);
        }
    }
}