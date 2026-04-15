using UnityEngine;

public sealed class StarterEquipment : MonoBehaviour
{
    public WeaponData starterSword;

    private void Start()
    {
        // Wait a tiny bit for the Inventory to initialize, then add and equip
        Inventory.Instance.AddWeapon(starterSword);
        Inventory.Instance.EquipWeapon(starterSword);
    }
}