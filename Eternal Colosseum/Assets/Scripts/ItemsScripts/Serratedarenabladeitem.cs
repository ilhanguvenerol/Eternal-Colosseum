using UnityEngine;

[CreateAssetMenu(fileName = "SerratedArenaBlade", menuName = "Inventory/Coliseum Items/Serrated Arena Blade")]
public class SerratedArenaBladeItem : ItemData
{
    [Header("Bleed Settings")]
    [Range(0f, 1f)] public float bleedChance = 0.15f;
    public float bleedDamagePerTick = 5f;
    public float bleedDuration = 3f;
    public float bleedTickRate = 0.5f;

    public override void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt)
    {
        if (Random.value > bleedChance) return;

        // Reuse or add the BleedEffect MonoBehaviour on the enemy
        BleedEffect bleed = targetEnemy.GetComponent<BleedEffect>()
                         ?? targetEnemy.AddComponent<BleedEffect>();

        bleed.StartBleed(bleedDamagePerTick, bleedDuration, bleedTickRate);
        Debug.Log($"[ITEM] Serrated Arena Blade — {targetEnemy.name} is BLEEDING!");
    }
}