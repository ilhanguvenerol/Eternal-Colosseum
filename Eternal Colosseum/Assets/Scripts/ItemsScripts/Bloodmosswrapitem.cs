using UnityEngine;

[CreateAssetMenu(fileName = "BloodmossWrap", menuName = "Inventory/Coliseum Items/Bloodmoss Wrap")]
public class BloodmossWrapItem : ItemData
{
    [Header("Regeneration Settings")]
    public float healPerSecond = 4.5f;
}