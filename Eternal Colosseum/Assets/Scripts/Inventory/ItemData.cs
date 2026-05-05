using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{

    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemRarity rarity;

    [Header("Stats")]
    public float bonusHealth;
    public float bonusSpeed;
    public float bonusDamage;
    public float bonusArmor;

    [Header("Economy")]
    public int price;
}


public enum ItemRarity
{
    Common,
    Rare,
    Epic
}