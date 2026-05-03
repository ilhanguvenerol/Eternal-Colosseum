using UnityEngine;

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
    public CharacterController Controller { get; private set; }
    public Animator            Animator   { get; private set; }

    // ── Private ───────────────────────────────────────────────────────────────

    private EnemyState _currentState;
    private bool       _initialised;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Animator   = GetComponent<Animator>();
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

    // ── Helpers used by multiple states ──────────────────────────────────────

    public float DistanceToPlayer()
        => Vector3.Distance(transform.position, Player.position);

    /// <summary>Move in world-space direction at given speed, gravity applied.</summary>
    public void Move(Vector3 worldDirection, float speed)
    {
        Vector3 motion = worldDirection.normalized * speed * Time.deltaTime;
        motion.y = -9.81f * Time.deltaTime; // simple gravity
        Controller.Move(motion);
    }

    /// <summary>Direction perpendicular to the player (for orbiting).</summary>
    public Vector3 PerpendicularToPlayer(float sign)
    {
        Vector3 toPlayer = (Player.position - transform.position).normalized;
        return Quaternion.AngleAxis(90f * sign, Vector3.up) * toPlayer;
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
