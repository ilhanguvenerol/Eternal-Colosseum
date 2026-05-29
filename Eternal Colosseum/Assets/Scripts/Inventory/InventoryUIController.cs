using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the 5-column inventory / shop UI.
/// Attach to InventoryWindow. Wire all slots in the Inspector.
/// Call InventoryUIController.Instance.RefreshUI() anytime inventory changes.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    // ── Column Content Areas (drag the "Content" child of each ScrollView here) ──
    [Header("Column Scroll Contents")]
    public Transform itemsContent;
    public Transform charmsContent;
    public Transform skillsContent;
    public Transform weaponsContent;

    // ── Description Panel 
    [Header("Description Panel")]
    public Image itemIconDisplay;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemRarityText;
    public TextMeshProUGUI itemStatsText;
    public Button equipButton;
    private TextMeshProUGUI _equipBtnLabel;

    // ── Prefab 
    [Header("Prefab")]
    public GameObject itemButtonPrefab;

    // ── Runtime
    private ScriptableObject _inspected; // whatever is currently selected


    //  Unity Lifecycle
    

    private void Awake()
{
    Instance = this;
    if (equipButton != null)
    {
        _equipBtnLabel = equipButton.GetComponentInChildren<TextMeshProUGUI>();
        equipButton.onClick.AddListener(OnEquipClicked);
        equipButton.gameObject.SetActive(false); // ← add this line
    }
}

    private void OnEnable()
    {
        StartCoroutine(RefreshUIDelayed());

    }
    private System.Collections.IEnumerator RefreshUIDelayed()
    {
        // Auto-refresh every time the panel is opened
        yield return null; // Small delay to ensure the panel is fully initialized
        RefreshUI();
    }

    
    //  Public API
   

    /// <summary>Call this after any inventory change to keep the UI in sync.</summary>
    public void RefreshUI()
    {
        ClearAllColumns();
        ResetDescriptionPanel();

        if (Inventory.Instance == null) return;

        // ── Items (passive, always active — no equip button shown) 
        foreach (ItemData item in Inventory.Instance.ownedItems)
        {
            string stats = BuildItemStats(item);
            SpawnButton(item, itemsContent, item.itemName, stats, item.itemIcon);
        }

        // ── Charms (equippable, max 3) 
        foreach (CharmData charm in Inventory.Instance.ownedCharms)
        {
            bool equipped = Inventory.Instance.equippedCharms.Contains(charm);
            string label = (equipped ? "[E] " : "") + charm.charmName;
            string stats = $"Bonus HP: +{charm.bonusHealth}  Bonus Dmg: +{charm.bonusDamage}\n" +
                            $"Debuff HP: -{charm.debuffHealth}  Debuff Dmg: -{charm.debuffDamage}";
            SpawnButton(charm, charmsContent, label, stats, null);
        }

        // ── Skills (equippable, max 1) 
        foreach (SkillData skill in Inventory.Instance.ownedSkills)
        {
            bool equipped = Inventory.Instance.equippedSkill == skill;
            string label = (equipped ? "[E] " : "") + skill.skillName;
            SpawnButton(skill, skillsContent, label, "Active Skill", null);
        }

        // ── Weapons (equippable, max 1) 
        foreach (WeaponData weapon in Inventory.Instance.ownedWeapons)
        {
            bool equipped = Inventory.Instance.equippedWeapon == weapon;
            string label = (equipped ? "[E] " : "") + weapon.weaponName;
            string stats = $"Base Damage: {weapon.baseDamage}";
            SpawnButton(weapon, weaponsContent, label, stats, null);
        }
    }

    
    //  Private Helpers
    

    private void SpawnButton(ScriptableObject data, Transform parent,
                             string label, string stats, Sprite icon)
    {
        GameObject go = Instantiate(itemButtonPrefab, parent);
        Button btn = go.GetComponent<Button>();

        // Set label text on the button
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = label;

        // Set small icon on the button if slot exists and sprite provided
        Image iconImg = go.transform.Find("ItemIconSmall")?.GetComponent<Image>();
        if (iconImg != null)
        {
            iconImg.sprite = icon;
            iconImg.enabled = icon != null;
        }

        // Click → show in description column
        btn.onClick.AddListener(() =>
        {
            _inspected = data;
            ShowDescription(label, icon, stats, data);
        });
    }

    private void ShowDescription(string name, Sprite icon, string stats, ScriptableObject data)
    {
        
        // Name
        itemNameText.text = name.Replace("[E] ", ""); // strip equipped marker for the title

        // Rarity
        if (data is ItemData item)
            itemRarityText.text = item.rarity.ToString();
        else if (data is WeaponData)
            itemRarityText.text = "Weapon";
        else if (data is CharmData)
            itemRarityText.text = "Charm";
        else if (data is SkillData)
            itemRarityText.text = "Skill";
        else
            itemRarityText.text = "";

        // Stats
        itemStatsText.text = stats;

        // Icon
        if (itemIconDisplay != null)
        {
            itemIconDisplay.sprite = icon;
            itemIconDisplay.enabled = icon != null;
        }

        // Equip button
        RefreshEquipButton(data);
    }

    private void RefreshEquipButton(ScriptableObject data)
    {
        if (equipButton == null) return;

        // Items are passive — hide equip button entirely
        if (data is ItemData)
        {
            equipButton.gameObject.SetActive(false);
            return;
        }

        equipButton.gameObject.SetActive(true);

        if (data is WeaponData weapon)
        {
            bool alreadyEquipped = Inventory.Instance.equippedWeapon == weapon;
            _equipBtnLabel.text = alreadyEquipped ? "Equipped [E]" : "Equip Weapon";
            equipButton.interactable = !alreadyEquipped;
        }
        else if (data is SkillData skill)
        {
            bool alreadyEquipped = Inventory.Instance.equippedSkill == skill;
            _equipBtnLabel.text = alreadyEquipped ? "Equipped [E]" : "Equip Skill";
            equipButton.interactable = !alreadyEquipped;
        }
        else if (data is CharmData charm)
        {
            bool alreadyEquipped = Inventory.Instance.equippedCharms.Contains(charm);
            bool slotsFull = Inventory.Instance.equippedCharms.Count >= Inventory.Instance.maxCharmSlots;

            if (alreadyEquipped)
            {
                _equipBtnLabel.text = "Unequip Charm";
                equipButton.interactable = true;
            }
            else if (slotsFull)
            {
                _equipBtnLabel.text = "Slots Full (3/3)";
                equipButton.interactable = false;
            }
            else
            {
                _equipBtnLabel.text = "Equip Charm";
                equipButton.interactable = true;
            }
        }
    }

    private void OnEquipClicked()
    {
        if (_inspected == null || Inventory.Instance == null) return;

        if (_inspected is WeaponData weapon)
            Inventory.Instance.EquipWeapon(weapon);
        else if (_inspected is SkillData skill)
            Inventory.Instance.EquipSkill(skill);
        else if (_inspected is CharmData charm)
        {
            if (Inventory.Instance.equippedCharms.Contains(charm))
                Inventory.Instance.UnequipCharm(charm);
            else
                Inventory.Instance.EquipCharm(charm);
        }

        // Refresh columns so  [E] markers update, then re-show the same item
        RefreshUI();
        if (_inspected != null)
        {
            string name = itemNameText.text;
            string stats = itemStatsText.text;
            Sprite icon = itemIconDisplay != null ? itemIconDisplay.sprite : null;
            ShowDescription(name, icon, stats, _inspected);
        }
    }

    private void ResetDescriptionPanel()
    {
        if (itemNameText) itemNameText.text = "Select an item";
        if (itemRarityText) itemRarityText.text = "";
        if (itemStatsText) itemStatsText.text = "";
        if (itemIconDisplay) itemIconDisplay.enabled = false;
        if (equipButton) equipButton.gameObject.SetActive(false);
        _inspected = null;
    }

    private void ClearAllColumns()
    {
        ClearColumn(itemsContent);
        ClearColumn(charmsContent);
        ClearColumn(skillsContent);
        ClearColumn(weaponsContent);
    }

    private void ClearColumn(Transform col)
    {
        if (col == null) return;
        foreach (Transform child in col)
            Destroy(child.gameObject);
    }

    private string BuildItemStats(ItemData item)
    {
        var lines = new List<string>();
        if (item.bonusDamage != 0) lines.Add($"Damage:  +{item.bonusDamage}");
        if (item.bonusHealth != 0) lines.Add($"Health:  +{item.bonusHealth}");
        if (item.bonusSpeed != 0) lines.Add($"Speed:   +{item.bonusSpeed}");
        if (item.bonusArmor != 0) lines.Add($"Armor:   +{item.bonusArmor}");
        if (lines.Count == 0) lines.Add("Passive Effect");
        return string.Join("\n", lines);
    }
}