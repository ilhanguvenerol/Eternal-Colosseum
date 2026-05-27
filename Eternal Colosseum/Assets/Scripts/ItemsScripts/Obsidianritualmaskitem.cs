using UnityEngine;

[CreateAssetMenu(fileName = "ObsidianRitualMask", menuName = "Inventory/Coliseum Items/Obsidian Ritual Mask")]
public class ObsidianRitualMaskItem : ItemData
{
    [Header("Mask Settings")]
    [Range(0f, 1f)] public float cooldownReduction = 0.25f; // 25% reduction

    public override void OnEquipEffect(GameObject player)
    {
        // Interfaces with PlayerCombatState spell cooldown timers.
        // Multiply any spell cooldown duration by (1f - cooldownReduction) when reading it.
        Debug.Log($"[ITEM] Obsidian Ritual Mask equipped. Spell cooldowns reduced by {cooldownReduction * 100}%.");
    }
}