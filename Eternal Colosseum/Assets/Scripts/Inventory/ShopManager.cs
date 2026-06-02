using TMPro;
using System.Collections;
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
    [SerializeField] private float commonChance = 60f;
    [SerializeField] private float rareChance = 30f;
    [SerializeField] private float epicChance = 10f;

    [Header("Current Shop Items")]
    public ItemData[] currentDisplayItems = new ItemData[3];

    [Header("UI - Slot 1")]
    [SerializeField] private Image itemIcon1;
    [SerializeField] private TextMeshProUGUI itemName1;
    [SerializeField] private TextMeshProUGUI itemStats1;
    [SerializeField] private TextMeshProUGUI itemPrice1;
    [SerializeField] private Image slotBackground1;

    [Header("UI - Slot 2")]
    [SerializeField] private Image itemIcon2;
    [SerializeField] private TextMeshProUGUI itemName2;
    [SerializeField] private TextMeshProUGUI itemStats2;
    [SerializeField] private TextMeshProUGUI itemPrice2;
    [SerializeField] private Image slotBackground2;

    [Header("UI - Slot 3")]
    [SerializeField] private Image itemIcon3;
    [SerializeField] private TextMeshProUGUI itemName3;
    [SerializeField] private TextMeshProUGUI itemStats3;
    [SerializeField] private TextMeshProUGUI itemPrice3;
    [SerializeField] private Image slotBackground3;

    [Header("UI - General")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI shopMessageText;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject inventoryCanvas;

    // Colors
    private readonly Color normalSlotColor = new Color(0.12f, 0.10f, 0.07f, 1f);
    private readonly Color purchasedColor = new Color(0.10f, 0.40f, 0.10f, 1f); // green flash
    private readonly Color errorColor = new Color(0.40f, 0.10f, 0.10f, 1f); // red flash
    private Color originalSlotColor;

    private void Start()
    {
        if(slotBackground1 != null) originalSlotColor = slotBackground1.color;
        AudioManager.Instance.PlayShopMusic();
        if (shopPanel != null)
            shopPanel.SetActive(true);

        StartCoroutine(InitShop());
    }

    private IEnumerator InitShop()
    {
        float timeout = 3f;
        while (Inventory.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("[SHOP] Inventory.Instance never became available!");
            yield break;
        }

        RollShopItems();
    }

    private void Update()
    {
        

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RerollShop();

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            inventoryCanvas.SetActive(true);
            bool isOpen = shopPanel.activeSelf;
            shopPanel.SetActive(!isOpen);
            // Hide inventory window when shop opens
            GameObject invWindow = shopPanel.transform.parent.Find("InventoryWindow")?.gameObject;
            if (invWindow != null) invWindow.SetActive(false);
            Time.timeScale = isOpen ? 1f : 0f;
        }

        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            BuyItem(0);

        if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
            BuyItem(1);

        if (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame)
            BuyItem(2);
    }

    private ItemRarity RollRarity()
    {
        float totalChance = commonChance + rareChance + epicChance;
        float roll = Random.Range(0f, totalChance);
        if (roll <= commonChance) return ItemRarity.Common;
        if (roll <= commonChance + rareChance) return ItemRarity.Rare;
        return ItemRarity.Epic;
    }

    private ItemData GetRandomItemByRarity(ItemRarity rarity)
    {
        List<ItemData> matchingItems = allAvailableItems.FindAll(item => item.rarity == rarity);
        if (matchingItems.Count == 0)
        {
            Debug.LogWarning($"[SHOP] No items found for rarity: {rarity}");
            return null;
        }
        return matchingItems[Random.Range(0, matchingItems.Count)];
    }

    public void RollShopItems()
    {
        if (allAvailableItems.Count < 3)
        {
            Debug.LogWarning("[SHOP] Not enough items to stock the shop!");
            return;
        }

        List<ItemData> selectedItems = new List<ItemData>();

        for (int i = 0; i < currentDisplayItems.Length; i++)
        {
            ItemRarity rolledRarity = RollRarity();
            ItemData selectedItem = GetRandomItemByRarity(rolledRarity);

            int attempts = 0;
            while (selectedItems.Contains(selectedItem) && attempts < 20)
            {
                selectedItem = GetRandomItemByRarity(rolledRarity);
                attempts++;
            }

            if (selectedItem != null)
            {
                currentDisplayItems[i] = selectedItem;
                selectedItems.Add(selectedItem);
            }
        }

        shopMessageText.text = "";
        ResetSlotColor(slotBackground1);
        ResetSlotColor(slotBackground2);
        ResetSlotColor(slotBackground3);

        RefreshSlot(0, itemIcon1, itemName1, itemStats1, itemPrice1);
        RefreshSlot(1, itemIcon2, itemName2, itemStats2, itemPrice2);
        RefreshSlot(2, itemIcon3, itemName3, itemStats3, itemPrice3);
        goldText.text = $"Gold: {Inventory.Instance.currentGold}G";
    }

    private void RefreshSlot(int index, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI statsText, TextMeshProUGUI priceText)
    {
        ItemData item = currentDisplayItems[index];
        if (item == null) return;

        // Reset name style in case it was set to SOLD
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Normal;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;

        if (icon != null)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = item.itemIcon != null;
        }

        nameText.text = item.itemName;
        statsText.text = GetItemStats(item);
        priceText.text = $"{item.price}G";

        // Re-enable buy button
        Image slotBg = index == 0 ? slotBackground1 : index == 1 ? slotBackground2 : slotBackground3;
        Button buyBtn = slotBg?.GetComponentInChildren<Button>(true);
        if (buyBtn != null) buyBtn.gameObject.SetActive(true);
    }

    private string GetItemStats(ItemData item)
    {
        string stats = "";
        if (item.bonusHealth > 0) stats += $"+{item.bonusHealth} HP\n";
        if (item.bonusDamage > 0) stats += $"+{item.bonusDamage} DMG\n";
        if (item.bonusArmor > 0) stats += $"+{item.bonusArmor} Armor\n";
        if (item.bonusSpeed > 0) stats += $"+{item.bonusSpeed} Speed\n";
        return stats.TrimEnd();
    }

    public void RerollShop()
    {
        if (Inventory.Instance.currentGold >= rerollCost)
        {
            Inventory.Instance.currentGold -= rerollCost;
            RollShopItems();
            shopMessageText.text = "Shop rerolled!";
            StartCoroutine(ClearShopMessage());
        }
        else
        {
            shopMessageText.text = "Not enough gold!";
            StartCoroutine(ClearShopMessage());
        }
    }


    public void BuyItem(int itemIndex)
    {
        ItemData selectedItem = currentDisplayItems[itemIndex];
        bool purchaseSuccessful = Inventory.Instance.BuyItem(selectedItem);

        Image slotBg = itemIndex == 0 ? slotBackground1 : itemIndex == 1 ? slotBackground2 : slotBackground3;

        if (purchaseSuccessful)
        {
            shopMessageText.text = $"Purchased: {selectedItem.itemName}!";
            goldText.text = $"Gold: {Inventory.Instance.currentGold}G";
            StartCoroutine(FlashSlot(slotBg, purchasedColor));

            // Clear the slot after flash
            currentDisplayItems[itemIndex] = null;
            StartCoroutine(ClearSlotAfterFlash(itemIndex));
        }
        else
        {
            shopMessageText.text = "Not enough gold!";
            StartCoroutine(FlashSlot(slotBg, errorColor));
        }

        StartCoroutine(ClearShopMessage());
    }

    private IEnumerator ClearSlotAfterFlash(int index)
    {
        yield return new WaitForSecondsRealtime(0.8f);

        Image icon = index == 0 ? itemIcon1 : index == 1 ? itemIcon2 : itemIcon3;
        TextMeshProUGUI name = index == 0 ? itemName1 : index == 1 ? itemName2 : itemName3;
        TextMeshProUGUI stats = index == 0 ? itemStats1 : index == 1 ? itemStats2 : itemStats3;
        TextMeshProUGUI price = index == 0 ? itemPrice1 : index == 1 ? itemPrice2 : itemPrice3;
        Image slotBg = index == 0 ? slotBackground1 : index == 1 ? slotBackground2 : slotBackground3;

        if (icon != null) icon.enabled = false;
        if (stats != null) stats.text = "";
        if (price != null) price.text = "";

        // Hide buy button
        Button buyBtn = slotBg?.GetComponentInChildren<Button>();
        if (buyBtn != null) buyBtn.gameObject.SetActive(false);

        // Bold SOLD text with reddish color
        if (name != null)
        {
            name.text = "SOLD";
            name.fontSize = 28;
            name.fontStyle = FontStyles.Bold;
            name.color = new Color(0.85f, 0.15f, 0.15f, 1f);
            name.alignment = TextAlignmentOptions.Center;
        }
    }

    private IEnumerator FlashSlot(Image slotBg, Color flashColor)
    {
        if (slotBg == null) yield break;
        slotBg.color = flashColor;
        yield return new WaitForSecondsRealtime(0.8f);
        float t = 0f;
        Color start = slotBg.color;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 2f;
            slotBg.color = Color.Lerp(start, originalSlotColor, t);
            yield return null;
        }
        slotBg.color = originalSlotColor;
    }

    private void ResetSlotColor(Image slotBg)
    {
        if (slotBg != null) slotBg.color = originalSlotColor;
    }

    private IEnumerator ClearShopMessage()
    {
        yield return new WaitForSeconds(2f);
        shopMessageText.text = "";
    }
}