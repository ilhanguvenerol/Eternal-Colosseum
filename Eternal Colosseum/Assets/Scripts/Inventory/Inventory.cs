using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [Header("Owned Items")]
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<CharmData> ownedCharms = new List<CharmData>();
    public List<SkillData> ownedSkills = new List<SkillData>();
    public List<WeaponData> ownedWeapons = new List<WeaponData>();

    [Header("Equipped")]
    public WeaponData equippedWeapon;
    public SkillData equippedSkill;
    public List<CharmData> equippedCharms = new List<CharmData>();

    [Header("Settings")]
    public int maxCharmSlots = 3;

    [Header("References")]
    public Transform weaponSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemData item)
    {
        ownedItems.Add(item);
    }

    public void AddCharm(CharmData charm)
    {
        ownedCharms.Add(charm);
    }

    public void AddSkill(SkillData skill)
    {
        ownedSkills.Add(skill);
    }

    public void AddWeapon(WeaponData weapon)
    {
        ownedWeapons.Add(weapon);
    }

    public bool EquipWeapon(WeaponData weapon)
    {
        // 1. Validation check
        if (weapon == null || !ownedWeapons.Contains(weapon)) return false;

        // 2. Clear the hand if something is already there
        if (weaponSlot != null && weaponSlot.childCount > 0)
        {
            foreach (Transform child in weaponSlot)
            {
                Destroy(child.gameObject);
            }
        }

        // 3. Update the data
        equippedWeapon = weapon;

        // 4. Physical Spawning
        if (weaponSlot != null && weapon.weaponPrefab != null)
        {
            // Instantiate the prefab
            GameObject newWeapon = Instantiate(weapon.weaponPrefab);

            // Parent it to the hand slot
            newWeapon.transform.SetParent(weaponSlot);

            // Reset position and rotation so it snaps to the hand
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
        }

        return true;
    }

    public bool EquipSkill(SkillData skill)
    {
        if (!ownedSkills.Contains(skill)) return false;
        equippedSkill = skill;
        return true;
    }

    public bool EquipCharm(CharmData charm)
    {
        if (!ownedCharms.Contains(charm)) return false;
        if (equippedCharms.Contains(charm)) return false;
        if (equippedCharms.Count >= maxCharmSlots) return false;
        equippedCharms.Add(charm);
        return true;
    }

    public bool UnequipCharm(CharmData charm)
    {
        return equippedCharms.Remove(charm);
    }

    public void UnequipWeapon()
    {
        equippedWeapon = null;
    }

    public void UnequipSkill()
    {
        equippedSkill = null;
    }

    public float GetTotalBonusHealth()
    {
        float total = 0;
        foreach (var item in ownedItems) total += item.bonusHealth;
        foreach (var charm in equippedCharms) total += charm.bonusHealth - charm.debuffHealth;
        return total;
    }

    public float GetTotalBonusSpeed()
    {
        float total = 0;
        foreach (var item in ownedItems) total += item.bonusSpeed;
        foreach (var charm in equippedCharms) total += charm.bonusSpeed - charm.debuffSpeed;
        return total;
    }

    public float GetTotalBonusDamage()
    {
        float total = 0;
        foreach (var item in ownedItems) total += item.bonusDamage;
        foreach (var charm in equippedCharms) total += charm.bonusDamage - charm.debuffDamage;
        return total;
    }

    public float GetTotalBonusArmor()
    {
        float total = 0;
        foreach (var item in ownedItems) total += item.bonusArmor;
        foreach (var charm in equippedCharms) total += charm.bonusArmor - charm.debuffArmor;
        return total;
    }

    public void ClearInventory()
    {
        ownedItems.Clear();
        ownedCharms.Clear();
        ownedSkills.Clear();
        ownedWeapons.Clear();
        equippedWeapon = null;
        equippedSkill = null;
        equippedCharms.Clear();
    }
}