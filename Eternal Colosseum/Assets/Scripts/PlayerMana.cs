using UnityEngine;
using UnityEngine.Events;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;

    [Header("Events")]
    public UnityEvent<float> onManaChanged;
    public UnityEvent onManaFull;

    private float currentMana = 0f;

    public float CurrentMana => currentMana;
    public float MaxMana     => maxMana;

    public void GainMana(float amount)
    {
        if (currentMana >= maxMana) return;

        float before = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);

        Debug.Log($"[Mana] +{amount} kazanıldı  |  Mana: {currentMana}/{maxMana}");
        onManaChanged?.Invoke(currentMana);

        if (before < maxMana && currentMana >= maxMana)
        {
            Debug.Log("[Mana] Mana tamamen doldu!");
            onManaFull?.Invoke();
        }
    }

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

    public bool HasEnoughMana(float amount)
    {
        return currentMana >= amount;
    }
}