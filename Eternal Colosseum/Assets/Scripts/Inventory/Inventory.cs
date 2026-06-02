using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Currency")]
    public int currentGold;

    [Header("References")]
    public Transform weaponSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            if (this.ownedItems.Count > 0)
                Instance.ownedItems.AddRange(this.ownedItems);

            Destroy(gameObject);
        }
    }


    //  NEW: BloodmossWrapItem passive heal tick 
    private void Update()
    {
        PlayerHealth ph = GetComponent<PlayerHealth>();
        if (ph == null) return;

        foreach (var item in ownedItems)
        {
            if (item is BloodmossWrapItem wrap)
                ph.Heal(wrap.healPerSecond * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────
    //  Item Management
    // ─────────────────────────────────────────


    public void AddItem(ItemData item)
    {
        if (item == null) return;
        ownedItems.Add(item);
        item.OnEquipEffect(gameObject); // Fire equip effects (FervorDraught, EchoSigil, ObsidianMask etc.)
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

    // ─────────────────────────────────────────
    //  Equip / Unequip
    // ─────────────────────────────────────────

    public bool EquipWeapon(WeaponData weapon)
    {
        if (weapon == null || !ownedWeapons.Contains(weapon)) return false;

        if (weaponSlot != null && weaponSlot.childCount > 0)
        {
            foreach (Transform child in weaponSlot)
                Destroy(child.gameObject);
        }

        equippedWeapon = weapon;

        if (weaponSlot != null && weapon.weaponPrefab != null)
        {
            GameObject newWeapon = Instantiate(weapon.weaponPrefab);
            newWeapon.transform.SetParent(weaponSlot);
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

    public bool UnequipCharm(CharmData charm) => equippedCharms.Remove(charm);

    public void UnequipWeapon() => equippedWeapon = null;

    public void UnequipSkill() => equippedSkill = null;

    // ─────────────────────────────────────────
    //  Stat Aggregation
    // ─────────────────────────────────────────

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

    // ─────────────────────────────────────────
    //  Utility
    // ─────────────────────────────────────────

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

    public bool BuyItem(ItemData itemToBuy)
    {
        if (currentGold < itemToBuy.price)
        {
            Debug.LogWarning($"[Shop] Not enough gold for {itemToBuy.itemName}! Need {itemToBuy.price - currentGold} more.");
            return false;
        }

        currentGold -= itemToBuy.price;
        ownedItems.Add(itemToBuy);
        itemToBuy.OnEquipEffect(gameObject); // Fire equip effect on purchase too

        Debug.Log($"[Shop] Bought {itemToBuy.itemName} for {itemToBuy.price} gold. Remaining: {currentGold}");
        return true;
    }
}