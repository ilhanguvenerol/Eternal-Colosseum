using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// MELEE IDLE
// Replicates the Arkham strafe: random orbit direction, re-rolled every second.
// This is what produces emergent flanking and support behaviour without
// needing explicit Flank / Support states.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeIdleState : EnemyState
{
    private float _directionTimer;
    private float _orbitSign; // +1 or -1

    public MeleeIdleState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        RollDirection();
    }

    private const float ChaseMultiplier = 3f;

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        // Player ran away — chase to close the gap, then resume orbiting
        if (dist > brain.engageStopDistance * ChaseMultiplier)
        {
            Vector3 dir = (player.position - brain.transform.position).normalized;
            brain.Move(dir, brain.engageSpeed);
            return;
        }

        _directionTimer -= Time.deltaTime;
        if (_directionTimer <= 0f)
            RollDirection();

        brain.Move(brain.PerpendicularToPlayer(_orbitSign), brain.orbitSpeed);
    }

    private void RollDirection()
    {
        _orbitSign      = Random.value > 0.5f ? 1f : -1f;
        _directionTimer = Random.Range(0.8f, 1.5f);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MELEE ENGAGE
// Moves straight toward the player. EnemyManager is responsible for calling
// BeginAttackApproach(); attack logic itself is handled outside this file.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeEngageState : EnemyState
{
    public MeleeEngageState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        if (dist > brain.engageStopDistance)
        {
            Vector3 dir = (player.position - brain.transform.position).normalized;
            brain.Move(dir, brain.engageSpeed);
        }
        // Once inside attack distance, the attack component takes over.
        // EnemyManager will call BeginRetreat() after the attack resolves.
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MELEE RETREAT
// Backs away until past a safe distance, then returns to idle orbiting.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeRetreatState : EnemyState
{
    private const float RetreatDistance = 4.5f;

    public MeleeRetreatState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        if (brain.DistanceToPlayer() < RetreatDistance)
        {
            brain.Move(-brain.transform.forward, brain.retreatSpeed);
        }
        else
        {
            brain.ChangeState(new MeleeIdleState(brain));
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GUARD
// Positions the melee enemy between the player and the ranged enemy it protects.
// The goal position is on the player→rangedEnemy segment, offset toward the
// ranged enemy by guardOffset. Re-evaluates every frame so it tracks both.
// Transitions back to MeleeIdleState if the ranged enemy dies.
// ─────────────────────────────────────────────────────────────────────────────
public class GuardState : EnemyState
{
    public GuardState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        EnemyBrain ranged = brain.guardTarget;

        // If the ranged enemy we were guarding is gone, return to orbit
        if (ranged == null || !ranged.isActiveAndEnabled)
        {
            brain.guardTarget = null;
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }

        // Goal: the point on the line from player to ranged enemy,
        // sitting guardOffset units away from the ranged enemy's position.
        Vector3 toRanged    = (ranged.transform.position - player.position).normalized;
        Vector3 goalPosition = ranged.transform.position - toRanged * brain.guardOffset;

        Vector3 dir  = (goalPosition - brain.transform.position);
        float   dist = dir.magnitude;

        // Only move if not already close enough to the goal
        if (dist > 0.3f)
            brain.Move(dir.normalized, brain.guardSpeed);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// STUNNED
// Freezes movement briefly after a hit. Returns to idle when done.
// ─────────────────────────────────────────────────────────────────────────────
public class StunnedState : EnemyState
{
    private const float StunDuration = 0.5f;
    private float _timer;

    public StunnedState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        _timer = StunDuration;
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            if (brain.enemyType == EnemyType.Melee)
                brain.ChangeState(new MeleeIdleState(brain));
            else
                brain.ChangeState(new RangedEngageState(brain));
        }
    }
}
