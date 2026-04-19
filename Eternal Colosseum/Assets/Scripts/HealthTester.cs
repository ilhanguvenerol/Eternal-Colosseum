using UnityEngine;
using UnityEngine.InputSystem;

public class HealthTester : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerMana playerMana;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMana   = GetComponent<PlayerMana>();
    }

    private void Update()
    {
        // ── Health ──────────────────────────────
        // H → 10 hasar al
        if (Keyboard.current.hKey.wasPressedThisFrame)
            playerHealth.TakeDamage(10f);

        // J → 20 can kazan
        if (Keyboard.current.jKey.wasPressedThisFrame)
            playerHealth.Heal(20f);

        // ── Mana ────────────────────────────────
        // M → düşmana vurmuş gibi mana kazan
        if (Keyboard.current.mKey.wasPressedThisFrame)
            playerMana.GainMana(playerMana.MaxMana * 0.15f);   // maxMana'nın %15'i

        // N → yetenek kullanmış gibi mana harca (50 mana)
        if (Keyboard.current.nKey.wasPressedThisFrame)
            playerMana.UseMana(50f);
    }
}