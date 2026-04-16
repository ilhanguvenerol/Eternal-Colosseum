using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Inventory/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;

    public SkillType skillType;
    public float damage;
    public float cooldown;
    public float range;
    public float duration;

    public int goldCost;
    public ItemRarity rarity;
}

public enum SkillType
{
    Offensive,
    Defensive,
    Utility
}