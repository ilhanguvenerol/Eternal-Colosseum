using UnityEngine;

[CreateAssetMenu(fileName = "PhantomWrap", menuName = "Inventory/Coliseum Items/Phantom Wrap")]
public class PhantomWrapItem : ItemData
{
    [Header("Veil Settings")]
    [Range(0f, 1f)] public float healthThreshold = 0.25f; // Activates below 25% HP
    public float veilDuration = 3f;

    // ── Static flag — readable from EnemyBrain to suppress attacks ───────────
    public static bool IsVeilActive = false;

    public override float OnTakeDamageEffect(GameObject player, float damageTaken)
    {
        if (IsVeilActive) return damageTaken;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        if (hp == null) return damageTaken;

        float healthAfterHit = hp.CurrentHealth - damageTaken;
        if (healthAfterHit / hp.MaxHealth < healthThreshold)
        {
            ActivateVeil(player);
        }

        return damageTaken;
    }

    private void ActivateVeil(GameObject player)
    {
        IsVeilActive = true;
        Debug.Log("[ITEM] Phantom Wrap activated — player fades from sight!");

        player.GetComponent<PlayerHealth>()?.StartPhantomVeil(veilDuration);
    }
}