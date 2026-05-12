using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    // ── Layer indices ─────────────────────────────────────────────────────────
    private const int BASE_LAYER   = 0;
    private const int COMBAT_LAYER = 1;

    // ── Parameter hashes ──────────────────────────────────────────────────────
    private static readonly int StateHash       = Animator.StringToHash("State");
    private static readonly int SpeedHash       = Animator.StringToHash("Speed");
    private static readonly int MoveXHash       = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash       = Animator.StringToHash("MoveZ");
    private static readonly int CombatStateHash = Animator.StringToHash("CombatState");

    // ── Base layer state values ───────────────────────────────────────────────
    public const int IDLE = 0;
    public const int WALK = 1;
    public const int DEAD = 2;

    // ── Combat layer state values ─────────────────────────────────────────────
    public const int COMBAT_NONE    = 0;
    public const int COMBAT_ATTACK  = 1;   // melee
    public const int COMBAT_DRAWBOW = 2;   // ranged — holds until PlayLoose()
    public const int COMBAT_LOOSE   = 3;   // ranged — plays once
    public const int COMBAT_HIT     = 4;   // both variants

    // ── Internal ─────────────────────────────────────────────────────────────
    private Animator _animator;
    private bool     _combatLocked;
    private bool     _dead;

    // ── Callbacks ─────────────────────────────────────────────────────────────
    // Subscribe from EnemyBrain or an attack component to sync game logic
    // with specific animation frames.
    public System.Action OnAttackHitFrame;   // melee: damage window opens
    public System.Action OnLooseReleased;    // ranged: spawn the arrow here
    public System.Action OnOneShotComplete;  // any combat clip finished

    // ── Unity ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // ── Base layer API ────────────────────────────────────────────────────────

    /// <summary>
    /// Call every frame from EnemyBrain.Update().
    /// Pass the NavMeshAgent velocity converted to local space.
    /// </summary>
    public void UpdateMovement(Vector3 localVelocity, float speed)
    {
        if (_dead) return;

        _animator.SetFloat(MoveXHash, localVelocity.x, 0.1f, Time.deltaTime);
        _animator.SetFloat(MoveZHash, localVelocity.z, 0.1f, Time.deltaTime);
        _animator.SetFloat(SpeedHash, speed,           0.1f, Time.deltaTime);
        _animator.SetInteger(StateHash, speed > 0.05f ? WALK : IDLE);
    }

    public void PlayDead()
    {
        _dead         = true;
        _combatLocked = false;
        _animator.SetInteger(StateHash,       DEAD);
        _animator.SetInteger(CombatStateHash, COMBAT_NONE);
    }

    // ── Combat layer API ──────────────────────────────────────────────────────

    /// <summary>Melee attack — plays once, returns to None via animation event.</summary>
    public void PlayAttack()
    {
        if (_combatLocked || _dead) return;
        _combatLocked = true;
        _animator.SetInteger(CombatStateHash, COMBAT_ATTACK);
    }

    /// <summary>
    /// Ranged step 1 — enter draw and hold.
    /// Call PlayLoose() when ready to fire.
    /// </summary>
    public void PlayDrawBow()
    {
        if (_combatLocked || _dead) return;
        _combatLocked = true;
        _animator.SetInteger(CombatStateHash, COMBAT_DRAWBOW);
    }

    /// <summary>
    /// Ranged step 2 — release the arrow. Must be in DrawBow first.
    /// </summary>
    public void PlayLoose()
    {
        if (!_combatLocked || _dead) return;
        _animator.SetInteger(CombatStateHash, COMBAT_LOOSE);
    }

    /// <summary>
    /// Flinch on hit. Interrupts combat lock so it always plays.
    /// </summary>
    public void PlayHit()
    {
        if (_dead) return;
        _combatLocked = false;
        _animator.SetInteger(CombatStateHash, COMBAT_HIT);
    }

    public void ExitCombat()
    {
        _combatLocked = false;
        _animator.SetInteger(CombatStateHash, COMBAT_NONE);
    }

    // ── Animation Events ──────────────────────────────────────────────────────

    public void AnimEvent_AttackHitFrame()  => OnAttackHitFrame?.Invoke();
    public void AnimEvent_LooseReleased()   => OnLooseReleased?.Invoke();
    public void AnimEvent_OneShotComplete() { ExitCombat(); OnOneShotComplete?.Invoke(); }

    // ── Helpers ───────────────────────────────────────────────────────────────
    public bool  IsCombatLocked => _combatLocked;
    public bool  IsDead         => _dead;

    public float GetCombatNormalizedTime()
        => _animator.GetCurrentAnimatorStateInfo(COMBAT_LAYER).normalizedTime;
}
