using System.Collections.Generic;
using UnityEngine;

public sealed class StarterEquipment : MonoBehaviour
{
    [Header("Starter Weapon")]
    public WeaponData starterSword;

    [Header("Starter Items (drag .asset files here to begin with them)")]
    public List<ItemData> starterItems = new List<ItemData>();

    // Awake runs before any Start() — so items are in the inventory
    // by the time PlayerHealth.Start() calls GetTotalBonusHealth()
    private void Awake()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[StarterEquipment] Inventory.Instance is null!");
            return;
        }

        // Weapon
        if (starterSword != null)
        {
            Inventory.Instance.AddWeapon(starterSword);
            Inventory.Instance.EquipWeapon(starterSword);
        }

        // Items — AddItem() calls OnEquipEffect() internally
        foreach (ItemData item in starterItems)
        {
            if (item != null)
                Inventory.Instance.AddItem(item);
        }

        Debug.Log($"[StarterEquipment] Ready. Items: {Inventory.Instance.ownedItems.Count} | Gold: {Inventory.Instance.currentGold}");
    }
}