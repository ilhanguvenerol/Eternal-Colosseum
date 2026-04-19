using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float manaRewardOnHit = 15f;  // oyuncuya verilecek mana

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────
    private float currentHealth;
    private bool isDead = false;

    // ─────────────────────────────────────────
    //  Properties
    // ─────────────────────────────────────────
    public bool IsDead => isDead;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // ─────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────

    /// <summary>Oyuncunun saldırı scripti buraya çağırır.</summary>
    public void TakeDamage(float amount, PlayerMana playerMana)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        Debug.Log($"[Enemy] Hasar alındı: {amount}  |  Kalan HP: {currentHealth}");

        // Oyuncuya mana ver
        playerMana?.GainMana(manaRewardOnHit);

        if (currentHealth <= 0f)
            Die();
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────
    private void Die()
    {
        isDead = true;
        Debug.Log("[Enemy] Düşman öldü!");
        gameObject.SetActive(false);  // şimdilik sadece gizle
    }
}