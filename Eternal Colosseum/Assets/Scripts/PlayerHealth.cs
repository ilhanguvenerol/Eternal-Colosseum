using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenRate = 5f;
    [SerializeField] private float regenDelay = 3f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.5f;

    // ─────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────
    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────
    private float currentHealth;
    private bool isDead = false;
    private bool isInvincible = false;
    private float lastDamageTime;

    // ─────────────────────────────────────────
    //  Properties
    // ─────────────────────────────────────────
    public float CurrentHealth => currentHealth;
    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }
    public bool IsDead => isDead;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (Inventory.Instance != null)
        {
            float bonusHealth = Inventory.Instance.GetTotalBonusHealth();
            maxHealth += bonusHealth;
            currentHealth = maxHealth;
            Debug.Log($"[STATS] Total Max Health initialized to: {maxHealth}");
        }
    }

    private void Update()
    {
        HandleRegen();
    }

    // ─────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead || isInvincible) return;

        float finalDamage = amount;
        if (Inventory.Instance != null)
        {
            foreach (ItemData item in Inventory.Instance.ownedItems)
                finalDamage = item.OnTakeDamageEffect(this.gameObject, finalDamage);
        }

        if (finalDamage <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0f, maxHealth);
        lastDamageTime = Time.time;

        Debug.Log($"[Health] Hasar alındı: {finalDamage}  |  Kalan HP: {currentHealth}");
        AudioManager.Instance.PlayPlayerHurt();
        onHealthChanged?.Invoke(currentHealth);
        StartCoroutine(InvincibilityRoutine());

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        Debug.Log($"[Health] İyileştirildi: {amount}  |  HP: {currentHealth}");
        onHealthChanged?.Invoke(currentHealth);
        AudioManager.Instance.PlayPlayerHeal();
    }

    public void RefreshBonusHealth()
    {
        if (Inventory.Instance == null) return;
        float newMax = 100f + Inventory.Instance.GetTotalBonusHealth();
        float diff = newMax - maxHealth;
        maxHealth = newMax;
        currentHealth = Mathf.Clamp(currentHealth + diff, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    // ── NEW: Required by SurgeonsVialItem ────────────────────────────────────
    /// <summary>
    /// Heals the player after a delay. Called by SurgeonsVialItem since
    /// ScriptableObjects cannot run Coroutines themselves.
    /// </summary>
    public void HealAfterDelay(float amount, float delay)
    {
        StartCoroutine(DelayedHeal(amount, delay));
    }

    private IEnumerator DelayedHeal(float amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        Heal(amount);
    }

    // ── NEW: Required by PhantomWrapItem ─────────────────────────────────────
    /// <summary>
    /// Activates the Phantom Wrap stealth veil for a duration.
    /// Called by PhantomWrapItem since ScriptableObjects cannot run Coroutines.
    /// EnemyBrain should check PhantomWrapItem.IsVeilActive before attacking.
    /// </summary>
    public void StartPhantomVeil(float duration)
    {
        StartCoroutine(PhantomVeilRoutine(duration));
    }

    private IEnumerator PhantomVeilRoutine(float duration)
    {
        PhantomWrapItem.IsVeilActive = true;
        Debug.Log("[Health] Phantom Wrap veil active.");
        yield return new WaitForSeconds(duration);
        PhantomWrapItem.IsVeilActive = false;
        Debug.Log("[Health] Phantom Wrap veil faded.");
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────
    private void Die()
    {
        isDead = true;
        Debug.Log("[Health] Oyuncu öldü!");
        onDeath?.Invoke();
    }

    private void HandleRegen()
    {
        if (!canRegenerate || isDead) return;
        if (currentHealth >= maxHealth) return;
        if (Time.time - lastDamageTime < regenDelay) return;

        currentHealth = Mathf.Clamp(currentHealth + regenRate * Time.deltaTime, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}