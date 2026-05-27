using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryCanvas;

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        bool isOpen = inventoryCanvas.activeSelf;
        inventoryCanvas.SetActive(!isOpen);

        // Pause time when inventory is open, resume when closed
        Time.timeScale = isOpen ? 1f : 0f;
    }
}