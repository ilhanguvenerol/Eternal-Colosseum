using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;

    public float bonusHealth;
    public float bonusSpeed;
    public float bonusDamage;
    public float bonusArmor;

    public int goldCost;
    public ItemRarity rarity;
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic
}