using UnityEngine;

[CreateAssetMenu(fileName = "FervorDraught", menuName = "Inventory/Coliseum Items/Fervor Draught")]
public class FervorDraughtItem : ItemData
{
    [Header("Stimulant Settings")]
    public float attackSpeedIncrease = 0.15f; // +15% attack animation speed

    public override void OnEquipEffect(GameObject player)
    {
        // Searches the scene for the player character by tag so we find the
        // actual Animator rather than the Inventory GameObject.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Animator anim = playerObj.GetComponentInChildren<Animator>();
        if (anim == null) return;

        float current = anim.GetFloat("AttackSpeedMultiplier");
        anim.SetFloat("AttackSpeedMultiplier", current + attackSpeedIncrease);

        Debug.Log($"[ITEM] Fervor Draught consumed. Attack speed multiplier now: {current + attackSpeedIncrease}");
    }
}