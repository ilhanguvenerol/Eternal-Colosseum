using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Melee, Ranged }

/// <summary>
/// Replaces EnemyScript's movement responsibility.
/// Owns the state machine and exposes shared references to every state.
/// Attack behaviour lives elsewhere (e.g. a separate EnemyAttack component).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnemyBrain : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Type")]
    public EnemyType enemyType = EnemyType.Melee;

    [Header("Variant")]
    public bool IsCelestial = false;

    [Header("Movement Speeds")]
    public float engageSpeed    = 5f;
    public float orbitSpeed     = 1.5f;
    public float retreatSpeed   = 2f;
    public float guardSpeed     = 3f;
    public float disengageSpeed = 4f;

    [Header("Distances")]
    public float engageStopDistance   = 2f;   // melee: stop attacking distance
    public float rangedFireDistance   = 10f;  // ranged: open fire inside this
    public float disengageThreshold   = 3f;   // ranged: flee if player is closer
    public float guardOffset          = 1.2f; // how close guard sits to the ranged enemy

    // ── Runtime state (read-only from states) ────────────────────────────────

    [Header("Debug — current state")]
    [SerializeField] private string currentStateName;

    /// <summary>The ranged enemy this melee Guard is protecting. Null if not guarding.</summary>
    [HideInInspector] public EnemyBrain guardTarget;

    // ── Shared components ─────────────────────────────────────────────────────

    public Transform           Player     { get; private set; }
    public NavMeshAgent        Agent      { get; private set; }
    public Animator            Animator   { get; private set; }

    // ── Private ───────────────────────────────────────────────────────────────

    private EnemyState _currentState;
    private bool       _initialised;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator   = GetComponent<Animator>();

        // Disable auto-rotation — we handle facing ourselves so enemies
        // always look at the player regardless of movement direction
        Agent.updateRotation = false;
    }

    private void Update()
    {
        if (!_initialised) return;
        FacePlayer();
        _currentState?.Update();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by SpawnManager immediately after instantiation.
    /// Boots the state machine once the player reference is available.
    /// </summary>
    public void SetPlayer(Transform player)
    {
        Player       = player;
        _initialised = true;

        if (enemyType == EnemyType.Melee)
            ChangeState(new MeleeIdleState(this));
        else
            ChangeState(new RangedEngageState(this));
    }

    public void ChangeState(EnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        currentStateName = newState.GetType().Name;
        _currentState.Enter();
    }

    // ── Movement API (used by states) ─────────────────────────────────────────

    /// <summary>
    /// Set NavMesh destination and speed in one call.
    /// States call this instead of manipulating the agent directly.
    /// </summary>
    public void MoveTo(Vector3 worldPosition, float speed)
    {
        Agent.isStopped = false;
        Agent.speed = speed;
        Agent.SetDestination(worldPosition);
    }

    /// <summary>Stop the agent in place.</summary>
    public void StopMoving()
    {
        Agent.isStopped = true;
        Agent.ResetPath();
    }


    // ── Helpers used by multiple states ──────────────────────────────────────

    public float DistanceToPlayer()
        => Player != null
            ? Vector3.Distance(transform.position, Player.position)
            : float.MaxValue;

    /// <summary>
    /// World position on a circle of radius r around the player,
    /// at the given angle in radians.
    /// </summary>
    public Vector3 OrbitPosition(float angleRad, float radius)
    {
        return Player.position + new Vector3(
            Mathf.Cos(angleRad) * radius,
            0f,
            Mathf.Sin(angleRad) * radius);
    }

    private void FacePlayer()
    {
        Vector3 dir = new Vector3(
            Player.position.x - transform.position.x,
            0f,
            Player.position.z - transform.position.z);

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // ── Manager-called entry points ───────────────────────────────────────────

    /// <summary>Called by EnemyManager when this enemy's attack turn arrives.</summary>
    public void BeginAttackApproach()
    {
        if (enemyType == EnemyType.Melee)
            ChangeState(new MeleeEngageState(this));
    }

    /// <summary>Called by EnemyManager after the attack resolves.</summary>
    public void BeginRetreat()
    {
        if (enemyType == EnemyType.Melee)
            ChangeState(new MeleeRetreatState(this));
    }

    /// <summary>Called by EnemyManager to assign guard duty over a ranged enemy.</summary>
    public void AssignGuard(EnemyBrain rangedEnemy)
    {
        guardTarget = rangedEnemy;
        ChangeState(new GuardState(this));
    }

    /// <summary>Called when this enemy takes a hit.</summary>
    public void OnHit()
    {
        ChangeState(new StunnedState(this));
    }

    // ── State query helpers (used by EnemyManager) ───────────────────────────

    public bool IsIdle()        => _currentState is MeleeIdleState;
    public bool IsRetreating()  => _currentState is MeleeRetreatState;
    public bool IsStunned()     => _currentState is StunnedState;
    public bool IsGuarding()    => _currentState is GuardState;
    public bool IsEngaging()    => _currentState is MeleeEngageState;
}
