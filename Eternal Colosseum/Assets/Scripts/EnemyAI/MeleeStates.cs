using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// MELEE IDLE
// Replicates the Arkham strafe: random orbit direction, re-rolled every second.
// This is what produces emergent flanking and support behaviour without
// needing explicit Flank / Support states.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeIdleState : EnemyState
{
    // Orbit radius matches engageStopDistance so the ring sits just outside attack range
    private const float OrbitRadiusMultiplier = 1.4f;
    private const float ChaseMultiplier = 3f;
    private const float AngleDriftSpeed = 0.4f;  // radians per second
    private const float AngleDriftInterval = 1.2f;  // seconds before drift direction re-rolls

    private float _angle;
    private float _driftSign;
    private float _driftTimer;

    public MeleeIdleState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        // Start at the angle closest to current position so there's no
        // sudden jump when entering this state
        Vector3 toSelf = brain.transform.position - player.position;
        toSelf.y = 0f;
        _angle = Mathf.Atan2(toSelf.z, toSelf.x);
        _driftSign = Random.value > 0.5f ? 1f : -1f;
        _driftTimer = AngleDriftInterval;
    }

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();
        float radius = brain.engageStopDistance * OrbitRadiusMultiplier;

        // Player ran away — chase to close the gap, then resume orbiting
        if (dist > brain.engageStopDistance * ChaseMultiplier)
        {
            brain.MoveTo(player.position, brain.engageSpeed);
            return;
        }

        // Drift angle
        _driftTimer -= Time.deltaTime;
        if (_driftTimer <= 0f)
        {
            _driftSign = Random.value > 0.5f ? 1f : -1f;
            _driftTimer = AngleDriftInterval;
        }

        _angle += _driftSign * AngleDriftSpeed * Time.deltaTime;

        brain.MoveTo(brain.OrbitPosition(_angle, radius), brain.orbitSpeed);
    }

    public override void Exit()
    {
        brain.StopMoving();
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// MELEE ENGAGE
// Moves straight toward the player. EnemyManager is responsible for calling
// BeginAttackApproach(); attack logic itself is handled outside this file.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeEngageState : EnemyState
{
    private const float AbandonMultiplier = 4f;

    public MeleeEngageState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.MoveTo(player.position, brain.engageSpeed);
    }

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        // Player ran far enough away — give up and return to orbit
        if (dist > brain.engageStopDistance * AbandonMultiplier)
        {
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }

        // Keep destination current as the player moves
        if (dist > brain.engageStopDistance)
        {
            brain.MoveTo(player.position, brain.engageSpeed);
        }
        else
        {
            // Inside attack distance — stop and let attack component take over
            brain.StopMoving();
        }
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

    public override void Enter()
    {
        SetRetreatDestination();
    }

    public override void Update()
    {
        if (brain.DistanceToPlayer() >= RetreatDistance)
        {
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }

        // Refresh destination each frame so it tracks away from a moving player
        SetRetreatDestination();
    }

    public override void Exit()
    {
        brain.StopMoving();
    }

    private void SetRetreatDestination()
    {
        Vector3 awayDir = (brain.transform.position - player.position).normalized;
        Vector3 retreatTo = brain.transform.position + awayDir * RetreatDistance;
        brain.MoveTo(retreatTo, brain.retreatSpeed);
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
            brain.MoveTo(goalPosition, brain.guardSpeed);
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
