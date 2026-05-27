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
    private GameObject _playerObj;

    // ── Parry State ───────────────────────────────────────────────────────────
    private bool _isParrying;

    /// <summary>
    /// True while the player is inside the active parry window.
    /// Enemy attacks check this before dealing damage.
    /// </summary>
    public bool IsParrying => _isParrying;

    private void Awake()
    {
        _input = new PlayerInputActions();
        _input.Combat.SwordAttack.performed += _ => TrySwordAttack();
        _input.Combat.Parry.performed += _ => TryParry();
        _input.Combat.Spell.performed += _ => TryCastSpell();
        _input.Combat.Spell.canceled += _ => TryExitSpell();

        // Cache in Awake so it's ready before any Start() runs
        _playerObj = GameObject.FindGameObjectWithTag("Player");
        if (_playerObj == null)
            Debug.LogError("[PlayerCombatState] No GameObject tagged 'Player' found! " +
                           "Select your Player in the Hierarchy and set its Tag to 'Player'.");
    }

    private void Start()
    {
        if (_startingSpell != null)
            EquipSpell(_startingSpell);
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    // ── Public API ────────────────────────────────────────────────────────────

    public void EquipSpell(SpellData spell)
    {
        _equippedSpell = spell;
        _player.Animator.EquipSpell(spell);
    }

    public SpellData EquippedSpell => _equippedSpell;

    // ── Combat Actions ────────────────────────────────────────────────────────

    private void TrySwordAttack()
    {
        if (_player.Animator.IsCombatLocked) return;

        // İlhan's animation call
        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_SWORD);

        if (Inventory.Instance == null || Inventory.Instance.equippedWeapon == null)
        {
            Debug.LogWarning("[COMBAT] Attack triggered but no weapon is equipped!");
            return;
        }

        // İlhan's damage calculation
        float totalDamage = Inventory.Instance.equippedWeapon.baseDamage
                          + Inventory.Instance.GetTotalBonusDamage();

        Debug.Log($"[COMBAT] Attacking with: {Inventory.Instance.equippedWeapon.weaponName}");
        Debug.Log($"[STATS] Total Calculated Damage: {totalDamage}");

        PlayerMana playerMana = _player.GetComponent<PlayerMana>();

        // Your optimized physics loop
        Collider[] hits = Physics.OverlapSphere(_player.transform.position, 4f);
        Debug.Log($"[PHYSICS] OverlapSphere caught {hits.Length} colliders in range.");

        System.Collections.Generic.HashSet<EnemyHealth> damagedEnemies =
            new System.Collections.Generic.HashSet<EnemyHealth>();

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = null;

            if (hit.transform.parent != null)
                enemy = hit.transform.parent.GetComponentInParent<EnemyHealth>();

            if (enemy == null && hit.transform.root != null)
                enemy = hit.transform.root.GetComponentInChildren<EnemyHealth>();

            if (enemy != null && !enemy.IsDead && !damagedEnemies.Contains(enemy))
            {
                damagedEnemies.Add(enemy);
                enemy.TakeDamage(totalDamage, playerMana);

                if (_playerObj != null)
                {
                    foreach (ItemData item in Inventory.Instance.ownedItems)
                        item.OnHitEffect(_playerObj, enemy.gameObject, totalDamage);
                }
            }
        }
    }

    /// <summary>
    /// Starts a short parry window.
    /// Enemy attacks during this time will stun the attacker instead of
    /// damaging the player.
    /// </summary>
    private void TryParry()
    {
        if (_player.Animator.IsCombatLocked) return;

        _isParrying = true;

        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_PARRY);

        // Close the parry window after a short duration
        Invoke(nameof(EndParry), 0.5f);
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Ends the active parry window.
    /// </summary>
    private void EndParry()
    {
        _isParrying = false;
    }
}