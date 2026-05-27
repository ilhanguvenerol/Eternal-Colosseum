using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float manaRewardOnHit = 15f;

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────
    private float currentHealth;
    private bool isDead = false;

    // ─────────────────────────────────────────
    //  Properties
    // ─────────────────────────────────────────
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

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
    public void TakeDamage(float amount, PlayerMana playerMana)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        Debug.Log($"[Enemy] Hasar alındı: {amount}  |  Kalan HP: {currentHealth}");

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

        // Disable FIRST so OverlapSphere-based items (PyreWraith, AshpyreDust)
        // cannot find this enemy as a target during chain reactions.
        gameObject.SetActive(false);

        Debug.Log("[Enemy] Düşman öldü!");

        // 1. Trigger kill items
        if (Inventory.Instance != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                foreach (ItemData item in Inventory.Instance.ownedItems)
                    item.OnKillEffect(playerObj, this.gameObject);
            }
        }

        // 2. Notify AI manager so the turn loop doesn't crash
        EnemyBrain brain = GetComponent<EnemyBrain>();
        if (brain != null)
        {
            EnemyManager manager = Object.FindAnyObjectByType<EnemyManager>();
            if (manager != null)
                manager.OnEnemyDied(brain);
        }
    }
}