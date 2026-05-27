using UnityEngine;
using UnityEngine.UIElements;




// Marked abstract and removed the CreateAssetMenu attribute
public abstract class ItemData : ScriptableObject
{
    [Header("Shop & UI Data")]
    public string itemName;
    public int price;
    public Sprite itemIcon;
    public ItemRarity rarity;

    [Header("Passive Stat Bonuses")]
    public float bonusHealth;
    public float bonusDamage;
    public float bonusSpeed;
    public float bonusArmor;

    // ── Polymorphic Triggers (Virtual so children only override what they need) ──

    public virtual void OnEquipEffect(GameObject player) { }

    public virtual void OnHitEffect(GameObject player, GameObject targetEnemy, float damageDealt) { }

    public virtual void OnKillEffect(GameObject player, GameObject killedEnemy) { }

    public virtual float OnTakeDamageEffect(GameObject player, float damageTaken)
    {
        return damageTaken; // Safely returns unmodified damage by default
    }
}
