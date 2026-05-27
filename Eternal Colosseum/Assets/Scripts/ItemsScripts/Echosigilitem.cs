using UnityEngine;

[CreateAssetMenu(fileName = "EchoSigil", menuName = "Inventory/Coliseum Items/Echo Sigil")]
public class EchoSigilItem : ItemData
{
    public override void OnEquipEffect(GameObject player)
    {
        // Hook into your SpellData charge system here.
        // Example: player.GetComponent<PlayerCombatState>().EquippedSpell.maxCharges += 1;
        Debug.Log("[ITEM] Echo Sigil carved. An extra spell charge echoes through you.");
    }
}