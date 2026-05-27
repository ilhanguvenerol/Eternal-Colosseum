using UnityEngine;

[CreateAssetMenu(fileName = "IronveilCloak", menuName = "Inventory/Coliseum Items/Ironveil Cloak")]
public class IronveilCloakItem : ItemData
{
    [Header("Cloak Settings")]
    public float sprintDamageReduction = 8f; // Flat damage blocked while sprinting

    public override float OnTakeDamageEffect(GameObject player, float damageTaken)
    {
        // PlayerController.IsRunHeld is true while the player holds the run button
        PlayerController controller = player.GetComponentInParent<PlayerController>();
        if (controller != null && controller.IsRunHeld)
        {
            float reduced = Mathf.Max(1f, damageTaken - sprintDamageReduction);
            Debug.Log($"[ITEM] Ironveil Cloak billowed — blocked {damageTaken - reduced} damage while sprinting.");
            return reduced;
        }

        return damageTaken;
    }
}