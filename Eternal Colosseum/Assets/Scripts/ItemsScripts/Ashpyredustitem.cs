using UnityEngine;

[CreateAssetMenu(fileName = "AshpyreDust", menuName = "Inventory/Coliseum Items/Ashpyre Dust")]
public class AshpyreDustItem : ItemData
{
    [Header("Ignition Settings")]
    public float igniteRadius = 5f;
    public float igniteDamage = 12f;

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        Debug.Log("[ITEM] Ashpyre Dust ignited — the fallen set the living ablaze!");

        Collider[] hits = Physics.OverlapSphere(killedEnemy.transform.position, igniteRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == killedEnemy) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            enemy?.TakeDamage(igniteDamage, null);
        }
    }
}