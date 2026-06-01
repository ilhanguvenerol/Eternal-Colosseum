using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Healing Spell Data")]
public class HealingSpellData : SpellData
{
    [Header("Healing Stats")]
    public float HealAmount = 25f;

    public override void ExecuteSpell(GameObject caster)
    {
        // 1. Grab the components directly off whoever cast the spell
        PlayerMana mana = caster.GetComponent<PlayerMana>();
        PlayerHealth health = caster.GetComponent<PlayerHealth>();

        if (mana != null && health != null)
        {
            // 2. Apply the math
            mana.UseMana(ManaCost);
            health.Heal(HealAmount);

            // 3. Play the Audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlayerHeal();
            }

            Debug.Log($"[Spell] Casted {SpellName}! Healed {HealAmount}");
        }
    }
}