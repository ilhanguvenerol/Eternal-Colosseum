using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Mana UI")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMana playerMana;

    private void Start()
    {
        playerHealth.onHealthChanged.AddListener(UpdateHealth);
        playerMana.onManaChanged.AddListener(UpdateMana);

        UpdateHealth(playerHealth.CurrentHealth);
        UpdateMana(playerMana.CurrentMana);
    }

    private void UpdateHealth(float current)
    {
        healthBarFill.fillAmount = current / playerHealth.MaxHealth;
        healthText.text = $"{Mathf.CeilToInt(current)} / {playerHealth.MaxHealth}";
    }

    private void UpdateMana(float current)
    {
        manaBarFill.fillAmount = current / playerMana.MaxMana;
        manaText.text = $"{Mathf.CeilToInt(current)} / {playerMana.MaxMana}";
    }
}