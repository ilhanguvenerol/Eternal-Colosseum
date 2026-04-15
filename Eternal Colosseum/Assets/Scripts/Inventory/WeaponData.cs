using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Visuals")]
    public string weaponName;
    public string description;
    public Sprite icon;
    public GameObject weaponPrefab; // The 3D model that spawns in the hand

    [Header("Weapon Settings")]
    public WeaponType weaponType;
    public Handedness handedness;

    [Header("Stats")]
    public float baseDamage;
    public float attackSpeed;
    public float range;

    [Header("Economy")]
    public int goldCost;
    public ItemRarity rarity;
}

// These must be outside the class brackets to be seen by other scripts
public enum WeaponType
{
    Sword,
    Axe,
    Spear
}

public enum Handedness
{
    OneHanded,
    TwoHanded
}