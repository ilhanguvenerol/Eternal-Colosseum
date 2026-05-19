using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    [Tooltip("Drag every item in the game here so the shop knows what exists!")]
    public List<ItemData> allAvailableItems;
    public int rerollCost = 20;

    [Header("Rarity Chances")]
    // Controls the chance of each rarity appearing in the shop
    [SerializeField] private float commonChance = 60f;
    [SerializeField] private float rareChance = 30f;
    [SerializeField] private float epicChance = 10f;

    [Header("Current Shop Items")]
    public ItemData[] currentDisplayItems = new ItemData[3]; // The 3 items on the shelf

    private void Start()
    {
        // Roll the first 3 items when the shop loads
        RollShopItems();
    }

    private void Update()
    {
        // [DEV CHEAT] Press 'R' to test the reroll!
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RerollShop();
        }
    }

    // Rolls a rarity based on the configured shop chances
    private ItemRarity RollRarity()
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= commonChance)
        {
            return ItemRarity.Common;
        }

        if (roll <= commonChance + rareChance)
        {
            return ItemRarity.Rare;
        }

        return ItemRarity.Epic;
    }

    // Picks 3 random items from the master list and puts them on the shelf.
    public void RollShopItems()
    {
        if (allAvailableItems.Count < 3)
        {
            Debug.LogWarning("[SHOP] Not enough items created to stock the shop!");
            return;
        }

        List<ItemData> tempItems = new List<ItemData>(allAvailableItems);

        for (int i = 0; i < currentDisplayItems.Length; i++)
        {
            // Pick a random item from the master list
            int randomIndex = Random.Range(0, tempItems.Count);
            currentDisplayItems[i] = tempItems[randomIndex];
            tempItems.RemoveAt(randomIndex);
        }

        Debug.Log($"[SHOP] Restocked! Shelf has: {currentDisplayItems[0].itemName}, {currentDisplayItems[1].itemName}, {currentDisplayItems[2].itemName}");
    }

    
    // Spends player gold to refresh the shop shelf.
    public void RerollShop()
    {
        if (Inventory.Instance.currentGold >= rerollCost)
        {
            Inventory.Instance.currentGold -= rerollCost;
            RollShopItems();
            Debug.Log($"[SHOP] Rerolled for {rerollCost}G. Remaining gold: {Inventory.Instance.currentGold}");
        }
        else
        {
            Debug.LogWarning("[SHOP] Not enough gold to reroll!");
        }
    }
}