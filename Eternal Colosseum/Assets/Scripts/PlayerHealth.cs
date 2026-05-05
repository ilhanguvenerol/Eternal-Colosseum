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
    [SerializeField] private float regenRate = 5f;        // saniyede kaç HP
    [SerializeField] private float regenDelay = 3f;       // hasar sonrası bekleme

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.5f; // hasar sonrası kısa dokunulmazlık

    // ─────────────────────────────────────────
    //  Events  (Inspector'dan UI / efekt bağlanabilir)
    // ─────────────────────────────────────────
    [Header("Events")]
    public UnityEvent<float> onHealthChanged;   // yeni HP değeri gönderir
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
    public float MaxHealth     => maxHealth;
    public bool  IsDead        => isDead;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Grant bonus health from inventory items/charms/skills
        if (Inventory.Instance != null)
        {
            float bonusHealth = Inventory.Instance.GetTotalBonusHealth();
            maxHealth += bonusHealth;
            currentHealth = maxHealth; // Bonus is applied at start, so we can set current to max directly

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

    /// <summary>Düşman scriptlerinden çağrılır.</summary>
    public void TakeDamage(float amount)
    {
        if (isDead || isInvincible) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        lastDamageTime = Time.time;

        Debug.Log($"[Health] Hasar alındı: {amount}  |  Kalan HP: {currentHealth}");

        onHealthChanged?.Invoke(currentHealth);

        StartCoroutine(InvincibilityRoutine());

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>İyileştirme potionları için.</summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        Debug.Log($"[Health] İyileştirildi: {amount}  |  HP: {currentHealth}");
        onHealthChanged?.Invoke(currentHealth);
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────
    private void Die()
    {
        isDead = true;
        Debug.Log("[Health] Oyuncu öldü!");
        onDeath?.Invoke();
        // TODO: ölüm animasyonu, respawn, game over ekranı buraya
    }

    private void HandleRegen()
    {
        if (!canRegenerate || isDead) return;
        if (currentHealth >= maxHealth) return;
        if (Time.time - lastDamageTime < regenDelay) return;

        currentHealth = Mathf.Clamp(currentHealth + regenRate * Time.deltaTime, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}