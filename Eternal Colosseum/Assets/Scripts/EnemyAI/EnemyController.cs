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
        public float AttackRange = 2.2f;
        public float MeleeThreshold = 3.5f;   // ranged: triggers Disengage

        [Header("Movement")]
        public float MoveSpeed = 3.5f;
        public float FlankArcAngle = 90f;     // degrees to orbit before snapping to point

        // ── Squad-managed ────────────────────────────────────────────────────
        [HideInInspector] public EnemySquadManager Squad;
        [HideInInspector] public PlayerTargetPoints TargetPoints;
        [HideInInspector] public EnemyController GuardTarget;
        [HideInInspector] public EnemyController AssignedGuard;

        // Set each frame by EnemySquadManager
        [HideInInspector] public Vector3 SupportTargetPosition;

        // Current engage point assignment
        public int AssignedPointIndex { get; private set; } = -1;
        public bool IsClosestPoint { get; private set; } = true;

        // ── Internal ─────────────────────────────────────────────────────────
        public NavMeshAgent Agent { get; private set; }
        public Animator Animator { get; private set; }
        public IEnemyState CurrentState { get; private set; }

        MeleeEngageState _meleeEngage;
        MeleeSupportState _meleeSupport;
        MeleeGuardState _meleeGuard;
        RangedEngageState _rangedEngage;
        RangedLooseState _rangedLoose;
        RangedDisengageState _rangedDisengage;

        // ── Unity ─────────────────────────────────────────────────────────────
        void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();

            _meleeEngage = new MeleeEngageState();
            _meleeSupport = new MeleeSupportState();
            _meleeGuard = new MeleeGuardState();
            _rangedEngage = new RangedEngageState();
            _rangedLoose = new RangedLooseState();
            _rangedDisengage = new RangedDisengageState();
        }

        void Start()
        {
            Agent.speed = MoveSpeed;

            TransitionTo(Variant == EnemyVariant.Melee
                ? (IEnemyState)_meleeEngage
                : _rangedEngage);
        }

        void Update()
        {
            CurrentState?.Execute(this);
        }

        // ── State helpers ─────────────────────────────────────────────────────
        public void TransitionTo(IEnemyState next)
        {
            CurrentState?.Exit(this);
            CurrentState = next;
            CurrentState.Enter(this);
        }

        public void GoMeleeEngage() => TransitionTo(_meleeEngage);
        public void GoMeleeSupport() => TransitionTo(_meleeSupport);
        public void GoMeleeGuard() => TransitionTo(_meleeGuard);
        public void GoRangedEngage() => TransitionTo(_rangedEngage);
        public void GoRangedLoose() => TransitionTo(_rangedLoose);
        public void GoRangedDisengage() => TransitionTo(_rangedDisengage);

        // Called by EnemySquadManager each frame for assigned enemies
        public void AssignEngagePoint(int pointIndex, bool isClosest)
        {
            AssignedPointIndex = pointIndex;
            IsClosestPoint = isClosest;

            if (CurrentState is not MeleeEngageState)
                GoMeleeEngage();
        }

        // ── Utility ───────────────────────────────────────────────────────────
        public Vector3 AssignedPointPosition
            => Squad != null ? Squad.GetEngagePointPosition(AssignedPointIndex) : transform.position;

        public float DistanceToPlayer()
            => TargetPoints != null
                ? Vector3.Distance(transform.position, TargetPoints.transform.position)
                : float.MaxValue;

        public bool PlayerInRange(float range) => DistanceToPlayer() <= range;

        public bool HasGuard()
            => AssignedGuard != null && AssignedGuard.CurrentState is MeleeGuardState;
    }

    public enum EnemyVariant { Melee, Ranged }
}