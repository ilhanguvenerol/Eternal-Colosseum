using UnityEngine;

[CreateAssetMenu(fileName = "IronWillWard", menuName = "Inventory/Coliseum Items/Iron Will Ward")]
public class IronWillWardItem : ItemData
{
    [Header("Ward Settings")]
    [Range(0f, 1f)] public float blockChance = 0.15f; // 15% chance to negate all damage

    public override float OnTakeDamageEffect(GameObject player, float incomingDamage)
    {
        if (Random.value <= blockChance)
        {
            Debug.Log("[ITEM] Iron Will Ward held — damage completely negated.");
            return 0f;
        }

        return incomingDamage;
    }
}