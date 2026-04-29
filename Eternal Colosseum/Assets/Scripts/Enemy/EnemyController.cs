using UnityEngine;
using UnityEngine.AI;

namespace EternalColosseum.EnemyAI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Variant")]
        public EnemyVariant Variant = EnemyVariant.Melee;
        public bool IsCelestial = false;

        [Header("Detection")]
        public float AttackRange       = 2.0f;   // melee swing / ranged fire range
        public float MeleeThreshold    = 3.5f;   // ranged: distance that triggers Disengage
        public float SupportRadius     = 8.0f;   // melee Support: orbit distance

        [Header("Movement")]
        public float MoveSpeed         = 3.5f;
        public float FlankArcAngle     = 90f;    // degrees to orbit before exiting Flank
        public float FlankOddsSupport  = 0.2f;   // 20% chance Flank → Support instead of Engage

        // ── References ───────────────────────────────────────────────────────
        [HideInInspector] public Transform       Player;
        [HideInInspector] public EnemyController GuardTarget;    // ranged unit this melee is guarding
        [HideInInspector] public EnemyController AssignedGuard;  // guard assigned to this ranged unit

        // ── Internal ─────────────────────────────────────────────────────────
        public NavMeshAgent Agent     { get; private set; }
        public Animator     Animator  { get; private set; }
        public IEnemyState  CurrentState { get; private set; }

        // Pre-allocated state instances (no per-frame allocation)
        MeleeEngageState    _meleeEngage;
        MeleeFlankState     _meleeFlank;
        MeleeSupportState   _meleeSupport;
        MeleeGuardState     _meleeGuard;
        RangedEngageState   _rangedEngage;
        RangedLooseState    _rangedLoose;
        RangedDisengageState _rangedDisengage;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        void Awake()
        {
            Agent    = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();

            _meleeEngage     = new MeleeEngageState();
            _meleeFlank      = new MeleeFlankState();
            _meleeSupport    = new MeleeSupportState();
            _meleeGuard      = new MeleeGuardState();
            _rangedEngage    = new RangedEngageState();
            _rangedLoose     = new RangedLooseState();
            _rangedDisengage = new RangedDisengageState();
        }

        void Start()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            Agent.speed = MoveSpeed;

            // Enter default state based on variant
            TransitionTo(Variant == EnemyVariant.Melee
                ? (IEnemyState)_meleeEngage
                : _rangedEngage);
        }

        void Update()
        {
            CurrentState?.Execute(this);
        }

        // ── State helpers ─────────────────────────────────────────────────────
        public void TransitionTo(IEnemyState nextState)
        {
            CurrentState?.Exit(this);
            CurrentState = nextState;
            CurrentState.Enter(this);
        }

        // Convenience accessors so states don't need to cache state objects
        public void GoMeleeEngage()    => TransitionTo(_meleeEngage);
        public void GoMeleeFlank()     => TransitionTo(_meleeFlank);
        public void GoMeleeSupport()   => TransitionTo(_meleeSupport);
        public void GoMeleeGuard()     => TransitionTo(_meleeGuard);
        public void GoRangedEngage()   => TransitionTo(_rangedEngage);
        public void GoRangedLoose()    => TransitionTo(_rangedLoose);
        public void GoRangedDisengage()=> TransitionTo(_rangedDisengage);

        // ── Utility ───────────────────────────────────────────────────────────
        public float DistanceToPlayer()
            => Player != null ? Vector3.Distance(transform.position, Player.position) : float.MaxValue;

        public bool PlayerInRange(float range)
            => DistanceToPlayer() <= range;

        public bool HasGuard()
            => AssignedGuard != null && AssignedGuard.CurrentState is MeleeGuardState;
    }

    public enum EnemyVariant { Melee, Ranged }
}
