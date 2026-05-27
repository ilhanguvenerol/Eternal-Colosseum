using UnityEngine;

[CreateAssetMenu(fileName = "PyreWraith", menuName = "Inventory/Coliseum Items/Pyre Wraith")]
public class PyreWraithItem : ItemData
{
    [Header("Wraith Settings")]
    public float explosionRadius = 7f;
    public float damageMultiplier = 2.5f; // 250% of killed enemy's max HP as AOE damage
    public GameObject wrathEffectPrefab; // Assign a dramatic soul-burst VFX in the Inspector

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        Debug.Log("[ITEM] Pyre Wraith detonated — a soul unleashed!");

        if (wrathEffectPrefab != null)
            Instantiate(wrathEffectPrefab, killedEnemy.transform.position, Quaternion.identity);

        // Base the explosion damage on the fallen enemy's max health for natural scaling
        EnemyHealth killedHP = killedEnemy.GetComponent<EnemyHealth>();
        float explosionDamage = (killedHP != null ? killedHP.MaxHealth : 50f) * damageMultiplier;

        Collider[] hits = Physics.OverlapSphere(killedEnemy.transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == killedEnemy) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            enemy?.TakeDamage(explosionDamage, null);
        }
    }
}