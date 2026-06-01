using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float manaRewardOnHit = 15f;

    [Header("Death Settings")]
    [SerializeField] private float deathDisableDelay = 1.5f;

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
        if (isDead) return;

        isDead = true;

        Debug.Log("[Enemy] Düşman öldü!");

        // 1. Trigger death animation and stop AI
        EnemyBrain brain = GetComponent<EnemyBrain>();

        if (brain != null)
        {
            brain.OnDeath();

            // Notify AI manager so the turn loop doesn't crash
            EnemyManager manager = Object.FindAnyObjectByType<EnemyManager>();

            if (manager != null)
                manager.OnEnemyDied(brain);
        }

        BossBrain boss = GetComponent<BossBrain>();

        if (boss != null)
        {
            boss.OnDeath();

            GameLoopManager.Instance.OnWaveCompleted();
        }

        // 2. Trigger kill items
        if (Inventory.Instance != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                foreach (ItemData item in Inventory.Instance.ownedItems)
                    item.OnKillEffect(playerObj, this.gameObject);
            }
        }

        // 3. Disable after death animation finishes
        StartCoroutine(DisableAfterDeathAnimation());
    }

    private IEnumerator DisableAfterDeathAnimation()
    {
        yield return new WaitForSeconds(deathDisableDelay);
        gameObject.SetActive(false);
    }
}