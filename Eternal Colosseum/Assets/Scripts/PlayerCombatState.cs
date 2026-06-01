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

        // Fallback reference grabber to prevent frame-one race conditions
        PlayerAnimator animator = (_player != null) ? _player.Animator : null;
        if (animator == null)
        {
            animator = GetComponent<PlayerAnimator>();
        }

        if (animator != null)
        {
            animator.EquipSpell(spell);
        }
        else
        {
            Debug.LogWarning("[PlayerCombatState] PlayerAnimator not ready yet for spell assignment.");
        }
    }

    public SpellData EquippedSpell => _equippedSpell;

    // ── Combat Actions ────────────────────────────────────────────────────────

    private void TrySwordAttack()
    {
        if (_player.Animator.IsCombatLocked) return;

        // ── SAFETY CHECK ─────────────────────────────────────────────────────
        // If the Inventory system isn't ready, or no weapon is equipped, 
        // just exit the method silently instead of warning the console.
        if (Inventory.Instance == null || Inventory.Instance.equippedWeapon == null)
        {
            return;
        }
        // ─────────────────────────────────────────────────────────────────────

        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_SWORD);

        float totalDamage = Inventory.Instance.equippedWeapon.baseDamage
                          + Inventory.Instance.GetTotalBonusDamage();

        PlayerMana playerMana = _player.GetComponent<PlayerMana>();

        Collider[] hits = Physics.OverlapSphere(_player.transform.position, 4f);
        
        System.Collections.Generic.HashSet<EnemyHealth> damagedEnemies =
            new System.Collections.Generic.HashSet<EnemyHealth>();

        foreach (Collider hit in hits)
        {
            // Ignore player itself
            if (hit.CompareTag("Player")) continue;

            // Ignore enemy projectiles
            if (hit.GetComponentInParent<EnemyProjectile>() != null) continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null)
                enemy = hit.GetComponent<EnemyHealth>();

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

        // 1. Check Mana Requirement
        PlayerMana playerMana = _player.GetComponent<PlayerMana>();
        if (playerMana != null && !playerMana.HasEnoughMana(_equippedSpell.ManaCost))
        {
            Debug.Log("[Spell] Not enough mana to cast " + _equippedSpell.SpellName);
            return;
        }

        // 2. DYNAMIC ANIMATION TYPE: Tell the animator exactly which animation state to play!
        Animator rawAnimator = _player.Animator.GetComponent<Animator>();
        if (rawAnimator != null)
        {
            rawAnimator.SetInteger("CombatState", _equippedSpell.AnimatorCombatState);
        }

        // 3. Play the spell execution state transitions
        _player.Animator.PlaySpell();

        // 4. Run the spell logic (Applies healing and deducts mana)
        _equippedSpell.ExecuteSpell(this.gameObject);
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