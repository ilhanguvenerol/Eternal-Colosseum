using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Melee, Ranged }
public enum EnemyPhase { Idle, Engaging, Retreating, Guarding, Stunned }

/// <summary>
/// Owns the NavMeshAgent, state machine, and animator feed for one enemy.
///
/// Key changes from previous version:
///   - EnemyAnimator is now single-layer (reference architecture).
///     FeedAnimator passes speed, strafeDir, and isStrafing flag so the
///     animator can switch between WalkBlend and StrafeBlend trees.
///   - StunnedState duration is driven by AnimEvent_HitComplete callback
///     so stun length always matches the actual clip length.
///   - Phase enum replaces all Is*() type-check methods.
///   - No wrapper entry-point methods — EnemyManager calls ChangeState directly.
///   - OnDeath disables further state updates cleanly.
/// </summary>
public class EnemyBrain : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Type")]
    public EnemyType enemyType = EnemyType.Melee;

    [Header("Variant")]
    public bool IsCelestial = false;

    [Header("Movement Speeds")]
    public float engageSpeed = 5f;
    public float orbitSpeed = 1.5f;
    public float retreatSpeed = 2f;
    public float guardSpeed = 3f;
    public float disengageSpeed = 4f;

    [Header("Distances")]
    public float engageStopDistance = 2f;
    public float rangedFireDistance = 10f;
    public float disengageThreshold = 3f;
    public float guardOffset = 1.2f;

    // ── Phase ─────────────────────────────────────────────────────────────────

    /// <summary>Set by each state in Enter(). Read by EnemyManager.</summary>
    public EnemyPhase Phase { get; set; } = EnemyPhase.Idle;

    // ── Debug ─────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField] private string currentStateName;

    // ── Guard target ──────────────────────────────────────────────────────────

    [HideInInspector] public EnemyBrain guardTarget;

    // ── Shared components ─────────────────────────────────────────────────────

    public Transform Player { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public EnemyAnimator EnemyAnimator { get; private set; }

    // ── Manager reference ─────────────────────────────────────────────────────

    [HideInInspector] public EnemyManager EnemyManager;

    // ── Animator feed hints (set by states each frame) ────────────────────────

    /// <summary>True while orbiting — tells animator to use StrafeBlend tree.</summary>
    public bool IsStrafing { get; set; } = false;

    /// <summary>Signed left/right value for StrafeBlend tree (-1 to +1).</summary>
    public float StrafeDir { get; set; } = 0f;

    // ── Private ───────────────────────────────────────────────────────────────

    private EnemyState _currentState;
    private bool _initialised;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        EnemyAnimator = GetComponent<EnemyAnimator>();
        Agent.updateRotation = false;
    }

    private void Update()
    {
        if (!_initialised) return;
        FacePlayer();
        _currentState?.Update();
        FeedAnimator();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>Called by SpawnManager immediately after instantiation.</summary>
    public void SetPlayer(Transform player)
    {
        Player = player;
        _initialised = true;

        if (enemyType == EnemyType.Melee)
            ChangeState(new MeleeIdleState(this));
        else
            ChangeState(new RangedEngageState(this));
    }

    // ── State machine ─────────────────────────────────────────────────────────

    public void ChangeState(EnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        currentStateName = newState.GetType().Name;
        _currentState.Enter();
    }

    // ── Movement API ──────────────────────────────────────────────────────────

    public void MoveTo(Vector3 worldPosition, float speed)
    {
        Agent.isStopped = false;
        Agent.speed = speed;
        Agent.SetDestination(worldPosition);
    }

    public void StopMoving()
    {
        Agent.isStopped = true;
        Agent.ResetPath();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public float DistanceToPlayer()
        => Player != null
            ? Vector3.Distance(transform.position, Player.position)
            : float.MaxValue;

    public Vector3 OrbitPosition(float angleRad, float radius)
        => Player.position + new Vector3(
            Mathf.Cos(angleRad) * radius,
            0f,
            Mathf.Sin(angleRad) * radius);

    public bool HasGuardAssigned()
        => EnemyManager != null && EnemyManager.HasGuardFor(this);

    // ── Damage entry points ───────────────────────────────────────────────────

    /// <summary>Called by the damage/combat component when this enemy is hit.</summary>
    public void OnHit()
    {
        EnemyAnimator?.PlayHit();

        EnemyState resumeState = enemyType == EnemyType.Melee
            ? (EnemyState)new MeleeIdleState(this)
            : (EnemyState)new RangedEngageState(this);

        ChangeState(new StunnedState(this, resumeState));
    }

    /// <summary>Called by the damage/combat component when this enemy dies.</summary>
    public void OnDeath()
    {
        _initialised = false; // stop state updates immediately
        StopMoving();
        EnemyAnimator?.PlayDeath();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void FacePlayer()
    {
        if (Player == null) return;
        Vector3 dir = new Vector3(
            Player.position.x - transform.position.x,
            0f,
            Player.position.z - transform.position.z);

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void FeedAnimator()
    {
        if (EnemyAnimator == null) return;

        float speed = Agent.velocity.magnitude;
        Vector3 localVel = transform.InverseTransformDirection(Agent.velocity);

        // Derive signed strafe direction from local horizontal velocity.
        // Positive = strafing right, negative = strafing left.
        float strafeDir = speed > 0.05f ? Mathf.Clamp(localVel.x / speed, -1f, 1f) : 0f;

        EnemyAnimator.UpdateMovement(speed, strafeDir, IsStrafing);
    }
}