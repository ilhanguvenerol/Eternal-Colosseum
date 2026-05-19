using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI shopText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private GameObject shopPanel;

    private void Start()
    {
        shopPanel.SetActive(false);
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

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
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

    // Returns a random item matching the rolled rarity
    private ItemData GetRandomItemByRarity(ItemRarity rarity)
    {
        List<ItemData> matchingItems =
            allAvailableItems.FindAll(item => item.rarity == rarity);

        if (matchingItems.Count == 0)
        {
            Debug.LogWarning($"[SHOP] No items found for rarity: {rarity}");
            return null;
        }

        int randomIndex = Random.Range(0, matchingItems.Count);
        return matchingItems[randomIndex];
    }

    // Picks 3 random items from the master list and puts them on the shelf.
    public void RollShopItems()
    {
        if (allAvailableItems.Count < 3)
        {
            Debug.LogWarning("[SHOP] Not enough items created to stock the shop!");
            return;
        }

        for (int i = 0; i < currentDisplayItems.Length; i++)
        {
            ItemRarity rolledRarity = RollRarity();
            ItemData selectedItem = GetRandomItemByRarity(rolledRarity);

            if (selectedItem != null)
            {
                currentDisplayItems[i] = selectedItem;
            }
        }

        Debug.Log($"[SHOP] Restocked! Shelf has: {currentDisplayItems[0].itemName}, {currentDisplayItems[1].itemName}, {currentDisplayItems[2].itemName}");

        shopText.text =
            $"=== SHOP ===\n\n" +
            $"{currentDisplayItems[0].itemName} - {currentDisplayItems[0].price}G\n" +
            $"{currentDisplayItems[1].itemName} - {currentDisplayItems[1].price}G\n" +
            $"{currentDisplayItems[2].itemName} - {currentDisplayItems[2].price}G\n\n" +
            $"Press R to reroll ({rerollCost}G)";

        goldText.text = $"Gold: {Inventory.Instance.currentGold}";
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