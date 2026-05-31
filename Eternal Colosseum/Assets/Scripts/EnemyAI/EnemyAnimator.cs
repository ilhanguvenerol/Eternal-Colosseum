using UnityEngine;

/// <summary>
/// Single-layer animator wrapper — modelled on the reference
/// (mixandjam/Batman-Arkham-Combat) enemy animator architecture.
///
/// Replacing the two-layer integer system with a single layer + triggers
/// eliminates the combat-layer/idle-layer desync that caused enemies to
/// play attack animations while orbiting.
///
/// ── Animator Controller setup ────────────────────────────────────────────────
///
/// SINGLE LAYER (Layer 0) — full body, no avatar mask needed:
///
///   Parameters:
///     InputMagnitude  (Float)   — drives WalkBlend speed
///     StrafeDirection (Float)   — drives StrafeBlend left/right
///     Strafe          (Bool)    — switches between WalkBlend and StrafeBlend
///     Punch           (Trigger) — fires attack from any state
///     Hit             (Trigger) — fires flinch from any state
///     Death           (Trigger) — fires death from any state
///
///   States:
///     WalkBlend    — 1D blend tree on InputMagnitude
///                    0.0 = Idle clip
///                    0.5 = Walk clip
///                    1.0 = Run clip (or duplicate Walk if no run)
///
///     StrafeBlend  — 1D blend tree on StrafeDirection
///                    -1 = StrafeLeft clip
///                     0 = Idle (or slow step) clip
///                    +1 = StrafeRight clip
///
///     Punch  — attack clip, speed multiplier 2.0
///     Hit    — flinch clip, speed multiplier 1.4
///     Death  — death clip,  speed multiplier 1.2  (no exit)
///
///   Transitions:
///     WalkBlend   → StrafeBlend : Strafe == true,  Has Exit Time OFF, Duration 0.15
///     StrafeBlend → WalkBlend   : Strafe == false, Has Exit Time OFF, Duration 0.15
///
///     Any State → Punch : Punch trigger, Can Transition To Self OFF, Duration 0.10
///     Any State → Hit   : Hit trigger,   Can Transition To Self OFF, Duration 0.05
///     Any State → Death : Death trigger, Can Transition To Self OFF, Duration 0.10
///
///     Punch → StrafeBlend : Has Exit Time ON, Exit Time 1.0, Duration 0.15
///     Hit   → StrafeBlend : Has Exit Time ON, Exit Time 1.0, Duration 0.10
///     (Death has no exit transition)
///
///   Animation Events to add in the clips:
///     Punch clip → AnimEvent_AttackHitFrame  at the impact frame
///               → AnimEvent_PunchComplete    at the last frame
///     Hit   clip → AnimEvent_HitComplete     at the last frame
///
/// ─────────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    // ── Parameter hashes ──────────────────────────────────────────────────────
    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");
    private static readonly int PunchHash = Animator.StringToHash("Punch");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int DrawBowHash = Animator.StringToHash("DrawBow");
    private static readonly int LooseHash = Animator.StringToHash("Loose");
    private static readonly int StunnedHash = Animator.StringToHash("Stunned");

    // ── Callbacks — subscribe from EnemyBrain / attack components ────────────

    /// <summary>Fired at the impact frame of the Punch clip.</summary>
    public System.Action OnAttackHitFrame;

    /// <summary>
    /// Fired at the last frame of the Punch clip.
    /// MeleeEngageState subscribes in Enter() and unsubscribes in Exit().
    /// </summary>
    public System.Action OnPunchComplete;

    /// <summary>
    /// Fired at the last frame of the Hit clip.
    /// StunnedState subscribes to know when to resume.
    /// </summary>
    public System.Action OnHitComplete;

    public System.Action OnLoose; // fires arrow here
    public System.Action OnLooseComplete; // LooseState hooks here to know when to fire again

    public System.Action OnStunnedComplete;

    // ── Internal ──────────────────────────────────────────────────────────────
    private Animator _animator;
    private bool _dead;

    private void Awake() => _animator = GetComponent<Animator>();

    // ── Movement API — called every frame by EnemyBrain ───────────────────────

    /// <summary>
    /// speed      : NavMeshAgent.velocity.magnitude
    /// strafeDir  : signed -1..+1 derived from local velocity X
    /// isStrafing : true while orbiting (MeleeIdleState), false otherwise
    /// </summary>
    public void UpdateMovement(float velocityX, float velocityZ)
    {
        if (_dead) return;
        _animator.SetFloat(VelocityXHash, velocityX, 0.1f, Time.deltaTime);
        _animator.SetFloat(VelocityZHash, velocityZ, 0.1f, Time.deltaTime);
    }

    // ── Combat API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Fires the Punch trigger. The Any State → Punch transition handles
    /// the interrupt. No external lock needed — the animator enforces it
    /// via exit time before returning to StrafeBlend.
    /// </summary>
    public void PlayPunch()
    {
        if (_dead) return;
        _animator.SetTrigger(PunchHash);
    }

    /// <summary>
    /// Fires the Hit trigger. Resets any pending Punch trigger first so
    /// a hit received mid-swing cleanly cancels the queued attack.
    /// </summary>
    public void PlayHit()
    {
        if (_dead) return;
        _animator.ResetTrigger(PunchHash);
        _animator.ResetTrigger(DrawBowHash);
        _animator.ResetTrigger(LooseHash);
        _animator.ResetTrigger(StunnedHash);
        _animator.SetTrigger(HitHash);
    }

    public void PlayStunned()
    {
        if (_dead) return;
        _animator.ResetTrigger(PunchHash);
        _animator.ResetTrigger(DrawBowHash);
        _animator.ResetTrigger(LooseHash);
        _animator.ResetTrigger(HitHash);
        _animator.SetTrigger(StunnedHash);
    }

    /// <summary>Fires death. All further API calls are silently ignored.</summary>
    public void PlayDeath()
    {
        _dead = true;
        _animator.ResetTrigger(PunchHash);
        _animator.ResetTrigger(HitHash);
        _animator.ResetTrigger(DrawBowHash);
        _animator.ResetTrigger(LooseHash);
        _animator.ResetTrigger(StunnedHash);
        _animator.SetTrigger(DeathHash);
    }

    public void PlayDrawBow()
    {
        if (_dead) return;
        _animator.ResetTrigger(LooseHash);
        _animator.SetTrigger(DrawBowHash);
    }

    public void PlayLoose()
    {
        if (_dead) return;
        _animator.SetTrigger(LooseHash);
    }
    // ── Animation Events — placed in clips via the Animation window ───────────

    /// <summary>Place at the impact frame of the Punch clip.</summary>
    public void AnimEvent_AttackHitFrame() => OnAttackHitFrame?.Invoke();

    /// <summary>Place at the last frame of the Punch clip.</summary>
    public void AnimEvent_PunchComplete() => OnPunchComplete?.Invoke();

    /// <summary>Place at the last frame of the Hit clip.</summary>
    public void AnimEvent_HitComplete() => OnHitComplete?.Invoke();

    public void AnimEvent_Loose() => OnLoose?.Invoke();
    public void AnimEvent_LooseComplete() => OnLooseComplete?.Invoke();

    public void AnimEvent_StunnedComplete() => OnStunnedComplete?.Invoke();
    // ── Helper ────────────────────────────────────────────────────────────────
    public bool IsDead => _dead;
}