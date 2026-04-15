using UnityEngine;

[CreateAssetMenu(fileName = "NewCharm", menuName = "Inventory/Charm")]
public class CharmData : ScriptableObject
{
    public string charmName;
    public string description;
    public Sprite icon;

    public float bonusHealth;
    public float bonusSpeed;
    public float bonusDamage;
    public float bonusArmor;

    public float debuffHealth;
    public float debuffSpeed;
    public float debuffDamage;
    public float debuffArmor;

    public int goldCost;
    public ItemRarity rarity;
}