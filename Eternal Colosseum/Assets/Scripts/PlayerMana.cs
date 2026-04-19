using UnityEngine;
using UnityEngine.Events;

public class PlayerMana : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaPerHit = 15f;      // düşmana her vuruşta kazanılan mana

    // ─────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────
    [Header("Events")]
    public UnityEvent<float> onManaChanged;    // UI bar için sonradan bağlanır
    public UnityEvent onManaFull;              // mana dolunca efekt için

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────
    private float currentMana = 0f;            // başlangıçta boş

    // ─────────────────────────────────────────
    //  Properties
    // ─────────────────────────────────────────
    public float CurrentMana => currentMana;
    public float MaxMana     => maxMana;

    // ─────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────

    /// <summary>Düşmana vurunca çağrılır.</summary>
    public void GainMana(float amount)
    {
        if (currentMana >= maxMana) return;

        float before = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);

        Debug.Log($"[Mana] +{amount} kazanıldı  |  Mana: {currentMana}/{maxMana}");
        onManaChanged?.Invoke(currentMana);

        // mana yeni doldu mu?
        if (before < maxMana && currentMana >= maxMana)
        {
            Debug.Log("[Mana] Mana tamamen doldu!");
            onManaFull?.Invoke();
        }
    }

    /// <summary>Yetenek kullanınca çağrılır.</summary>
    public void UseMana(float amount)
    {
        if (!HasEnoughMana(amount))
        {
            Debug.Log("[Mana] Yeterli mana yok!");
            return;
        }

        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
        Debug.Log($"[Mana] -{amount} kullanıldı  |  Mana: {currentMana}/{maxMana}");
        onManaChanged?.Invoke(currentMana);
    }

    /// <summary>Yetenek kullanılabilir mi kontrolü.</summary>
    public bool HasEnoughMana(float amount)
    {
        return currentMana >= amount;
    }
}