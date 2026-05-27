using UnityEngine;

[CreateAssetMenu(fileName = "AshpyreDust", menuName = "Inventory/Coliseum Items/Ashpyre Dust")]
public class AshpyreDustItem : ItemData
{
    [Header("Ignition Settings")]
    public float igniteRadius = 5f;
    public float igniteDamage = 12f;
    public GameObject igniteEffectPrefab; // Assign a fire burst VFX in the Inspector

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        Debug.Log("[ITEM] Ashpyre Dust ignited — the fallen set the living ablaze!");

        if (igniteEffectPrefab != null)
            Instantiate(igniteEffectPrefab, killedEnemy.transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(killedEnemy.transform.position, igniteRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == killedEnemy) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            enemy?.TakeDamage(igniteDamage, null);
        }
    }
}