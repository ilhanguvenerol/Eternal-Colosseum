using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryCanvas;
    public GameObject inventoryWindow;
    public GameObject shopPanel;

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            OpenInventory();
    }

    public void OpenInventory()
    {
        bool isOpen = inventoryWindow.activeSelf;

        // Make sure canvas is on
        inventoryCanvas.SetActive(true);

        // Toggle inventory, always hide shop
        inventoryWindow.SetActive(!isOpen);
        if (shopPanel != null) shopPanel.SetActive(false);

        // Pause time when inventory is open
        Time.timeScale = isOpen ? 1f : 0f;
    }
}