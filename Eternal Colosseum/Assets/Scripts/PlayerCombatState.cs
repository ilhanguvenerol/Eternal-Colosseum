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

        if (Inventory.Instance == null || Inventory.Instance.equippedWeapon == null)
        {
            return;
        }

        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_SWORD);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerAttack();
        }

        float totalDamage = Inventory.Instance.equippedWeapon.baseDamage
                          + Inventory.Instance.GetTotalBonusDamage();

        PlayerMana playerMana = _player.GetComponent<PlayerMana>();

        Collider[] hits = Physics.OverlapSphere(_player.transform.position + _player.transform.forward * 1.0f, 1.0f);

        System.Collections.Generic.HashSet<EnemyHealth> damagedEnemies =
            new System.Collections.Generic.HashSet<EnemyHealth>();

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) continue;
            if (hit.GetComponentInParent<EnemyProjectile>() != null) continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>() ?? hit.GetComponent<EnemyHealth>();

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

    private void TryParry()
    {
        if (_player.Animator.IsCombatLocked) return;

        _isParrying = true;
        _player.Animator.PlayCombatOneShot(PlayerAnimator.COMBAT_PARRY);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerParry();
        }

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

        PlayerMana playerMana = _player.GetComponent<PlayerMana>();
        if (playerMana != null && !playerMana.HasEnoughMana(_equippedSpell.ManaCost))
        {
            Debug.Log("[Spell] Not enough mana to cast " + _equippedSpell.SpellName);
            return;
        }

        // Search the player model hierarchy dynamically to find the graphics animator component safely
        Animator rawAnimator = _player.Animator.GetComponentInChildren<Animator>();
        if (rawAnimator != null)
        {
            // Case-Insulated Parameters to force execution across all capitalization formats
            rawAnimator.SetInteger("CombatState", _equippedSpell.AnimatorCombatState);
            rawAnimator.SetInteger("SpellVariant", _equippedSpell.AnimatorSpellVariant);
        }

        _player.Animator.PlaySpell();
        _equippedSpell.ExecuteSpell(this.gameObject);
    }

    private void TryExitSpell()
    {
        _player.Animator.ExitCombat();
    }

    private void EndParry()
    {
        _isParrying = false;
    }
}