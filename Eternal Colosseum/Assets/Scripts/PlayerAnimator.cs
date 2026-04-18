// PlayerAnimator.cs

using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // ── Layer indices ─────────────────────────────────────────────────────────
    private const int BASE_LAYER = 0;
    private const int COMBAT_LAYER = 1;

    // ── Parameter hashes ──────────────────────────────────────────────────────
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int CombatStateHash = Animator.StringToHash("CombatState");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    // ── Base layer state values ───────────────────────────────────────────────
    public const int IDLE = 0;
    public const int WALK = 1;
    public const int RUN = 2;
    public const int DASH = 3;

    // ── Combat layer state values ─────────────────────────────────────────────
    public const int COMBAT_NONE = 0;
    public const int COMBAT_SWORD = 1;
    public const int COMBAT_PARRY = 2;
    public const int COMBAT_SPELL = 3;

    // ── Spell clip placeholder name ───────────────────────────────────────────
    // This must match the name of the placeholder clip assigned to the
    // SpellBase state in your Animator. Any dummy clip works — name matters.
    private const string SPELL_PLACEHOLDER = "SpellBase";

    private Animator _animator;
    private AnimatorOverrideController _overrideController;

    private bool _combatLocked;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Wrap the original controller so we can swap clips at runtime
        // without modifying the original Animator asset
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
    }

    // ── Base layer API ────────────────────────────────────────────────────────

    public void SetState(int state) => _animator.SetInteger(StateHash, state);
    public void SetSpeed(float speed) => _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);

    // ── Combat layer API ──────────────────────────────────────────────────────

    public void PlayCombatOneShot(int combatState)
    {
        if (_combatLocked) return;
        _combatLocked = true;
        _animator.SetInteger(CombatStateHash, combatState);
    }

    /// <summary>
    /// Equips a spell — swaps the SpellBase clip to the spell's unique animation.
    /// Call this when the player selects a spell, not when they cast it.
    /// </summary>
    public void EquipSpell(SpellData spell)
    {
        if (spell == null || spell.CastAnimation == null)
        {
            Debug.LogWarning($"[PlayerAnimator] SpellData '{spell?.SpellName}' has no CastAnimation assigned.");
            return;
        }

        _overrideController[SPELL_PLACEHOLDER] = spell.CastAnimation;
    }

    /// <summary>
    /// Starts playing the equipped spell animation. EquipSpell() must be
    /// called first — otherwise the placeholder clip plays.
    /// </summary>
    public void PlaySpell()
    {
        if (_combatLocked) return;
        _animator.SetInteger(CombatStateHash, COMBAT_SPELL);
    }

    public void ExitCombat()
    {
        _combatLocked = false;
        _animator.SetInteger(CombatStateHash, COMBAT_NONE);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(AttackTriggerHash);
    }

    // ── Animation Event ───────────────────────────────────────────────────────

    /// <summary>
    /// Add this as an Animation Event on the last frame of SwordAttack and Parry.
    /// </summary>
    public void OnOneShotComplete() => ExitCombat();

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool IsCombatLocked => _combatLocked;

    public float GetCombatNormalizedTime() =>
        _animator.GetCurrentAnimatorStateInfo(COMBAT_LAYER).normalizedTime;

    public void PlayImmediate(string stateName, int layer = BASE_LAYER) =>
        _animator.Play(stateName, layer, 0f);
}