using UnityEngine;

[CreateAssetMenu(fileName = "ShatteredLegionShield", menuName = "Inventory/Coliseum Items/Shattered Legion Shield")]
public class ShatteredLegionShieldItem : ItemData
{
    [Header("Shield Settings")]
    public float flatReduction = 5f; // Subtracts 5 from every hit

    public override float OnTakeDamageEffect(GameObject player, float damageTaken)
    {
        float finalDamage = Mathf.Max(1f, damageTaken - flatReduction);
        Debug.Log($"[ITEM] Shattered Legion Shield absorbed {damageTaken - finalDamage} damage.");
        return finalDamage;
    }
}