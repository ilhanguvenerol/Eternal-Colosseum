// PlayerCombatState.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _player;

    [Header("Starting Spell (optional)")]
    [SerializeField] private SpellData _startingSpell;

    private PlayerInputActions _input;
    private SpellData _equippedSpell;

    private void Awake()
    {
        _input = new PlayerInputActions();

        _input.Combat.SwordAttack.performed += _ => TrySwordAttack();
        _input.Combat.Parry.performed += _ => TryParry();
        _input.Combat.Spell.performed += _ => TryCastSpell();    // single Spell action now
        _input.Combat.Spell.canceled += _ => TryExitSpell();
    }

    private void Start()
    {
        if (_startingSpell != null)
            EquipSpell(_startingSpell);
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    // ── Public API (call from inventory system later) ─────────────────────────

    /// <summary>
    /// Equip a new spell. Safe to call anytime outside of combat.
    /// The inventory system will call this when the player swaps spells.
    /// </summary>
    public void EquipSpell(SpellData spell)
    {
        _equippedSpell = spell;
        _player.Animator.EquipSpell(spell);   // swaps the animation clip
    }

    public SpellData EquippedSpell => _equippedSpell;

    // ── Combat Actions ────────────────────────────────────────────────────────

    private void TrySwordAttack()
{
    if (_player.Animator.IsCombatLocked) return;

    _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_SWORD);

    if (Inventory.Instance != null && Inventory.Instance.equippedWeapon != null)
    {
        float totalDamage = Inventory.Instance.equippedWeapon.baseDamage + Inventory.Instance.GetTotalBonusDamage();

        Debug.Log($"[COMBAT] Attacking with: {Inventory.Instance.equippedWeapon.weaponName}");
        Debug.Log($"[STATS] Total Calculated Damage: {totalDamage}");

        PlayerMana playerMana = _player.GetComponent<PlayerMana>();

        Collider[] hits = Physics.OverlapSphere(_player.transform.position, 2f);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(totalDamage, playerMana);
            }
        }
    }
    else
    {
        Debug.LogWarning("[COMBAT] Attack triggered but no weapon is equipped!");
    }
}

    private void TryParry()
    {
        if (_player.Animator.IsCombatLocked) return;
        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_PARRY);
    }

    private void TryCastSpell()
    {
        if (_player.Animator.IsCombatLocked) return;
        if (_equippedSpell == null)
        {
            Debug.Log("[PlayerCombatState] No spell equipped.");
            return;
        }

        _player.Animator.PlaySpell();
    }

    private void TryExitSpell()
    {
        _player.Animator.ExitCombat();
    }
}