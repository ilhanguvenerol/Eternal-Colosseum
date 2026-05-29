using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// MELEE IDLE
// Orbits the player using the Arkham strafe pattern.
// Sets brain.IsStrafing = true so EnemyBrain.FeedAnimator pushes the
// StrafeBlend tree — the animator plays left/right strafe clips.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeIdleState : EnemyState
{
    private const float OrbitRadiusMultiplier = 2.5f;
    private const float ChaseMultiplier = 3f;
    private const float AngleDriftSpeed = 0.4f;
    private const float AngleDriftInterval = 1.2f;

    private float _angle;
    private float _driftSign;
    private float _driftTimer;

    public MeleeIdleState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.Phase = EnemyPhase.Idle;

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

        // Player ran away — chase to close the gap, then resume orbiting.
        if (dist > brain.engageStopDistance * ChaseMultiplier)
        {
            brain.MoveTo(player.position, brain.engageSpeed);
            return;
        }

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
// Moves toward the player. When inside attack range, plays the punch animation
// and waits for AnimEvent_PunchComplete before transitioning out.
//
// Subscribes to OnPunchComplete in Enter() and unsubscribes in Exit() so the
// callback is scoped only to this enemy while it is actively engaging.
// The animator's own exit-time transition (Punch → StrafeBlend) is the
// visual gate — OnPunchComplete fires from AnimEvent at that same moment.
// ─────────────────────────────────────────────────────────────────────────────
public class MeleeEngageState : EnemyState
{
    private const float AbandonMultiplier = 4f;
    private bool _attackStarted;

    public MeleeEngageState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.Phase = EnemyPhase.Engaging;
        _attackStarted = false;

        // Safe check: Only subscribe if the animator exists
        if (brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnPunchComplete += OnPunchFinished;

        brain.MoveTo(player.position, brain.engageSpeed);
    }

    public override void Update()
    {
        if (_attackStarted) return;

        float dist = brain.DistanceToPlayer();

        if (dist > brain.engageStopDistance * AbandonMultiplier)
        {
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }

        if (dist > brain.engageStopDistance)
        {
            brain.MoveTo(player.position, brain.engageSpeed);
        }
        else
        {
            brain.StopMoving();
            _attackStarted = true;

            // Safe check: Play punch, or instantly finish if no animator
            if (brain.EnemyAnimator != null)
                brain.EnemyAnimator.PlayPunch();
            else
                OnPunchFinished();
        }
    }

    public override void Exit()
    {
        // Safe check
        if (brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnPunchComplete -= OnPunchFinished;
    }

    private void OnPunchFinished()
    {
        brain.ChangeState(new MeleeIdleState(brain));
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
        brain.Phase = EnemyPhase.Retreating;
        SetRetreatDestination();
    }

    public override void Update()
    {
        if (brain.DistanceToPlayer() >= RetreatDistance)
        {
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }
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
// Positions the melee enemy between the player and its assigned ranged enemy.
// Releases back to idle if the ranged enemy dies.
// ─────────────────────────────────────────────────────────────────────────────
public class GuardState : EnemyState
{
    public GuardState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.Phase = EnemyPhase.Guarding;
    }

    public override void Update()
    {
        EnemyBrain ranged = brain.guardTarget;

        if (ranged == null || !ranged.isActiveAndEnabled)
        {
            brain.guardTarget = null;
            brain.ChangeState(new MeleeIdleState(brain));
            return;
        }

        Vector3 toRanged = (ranged.transform.position - player.position).normalized;
        Vector3 goalPosition = ranged.transform.position - toRanged * brain.guardOffset;

        if ((goalPosition - brain.transform.position).magnitude > 0.3f)
            brain.MoveTo(goalPosition, brain.guardSpeed);
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// STUNNED
// Freezes movement after a hit and waits for the Hit animation to finish
// via the AnimEvent_HitComplete callback — stun duration matches clip length
// exactly, no hardcoded timer needed.
//
// Subscribes to OnHitComplete in Enter() and unsubscribes in Exit().
// The resume state is injected via constructor so this class has no
// knowledge of EnemyType.
// ─────────────────────────────────────────────────────────────────────────────
public class StunnedState : EnemyState
{
    private readonly EnemyState _resumeState;

    public StunnedState(EnemyBrain brain, EnemyState resumeState) : base(brain)
    {
        _resumeState = resumeState;
    }

    public override void Enter()
    {
        brain.Phase = EnemyPhase.Stunned;
        brain.StopMoving();

        if (brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnHitComplete += OnHitAnimationFinished;
        else
            OnHitAnimationFinished(); // Instantly un-stun if missing component
    }

    public override void Exit()
    {
        if (brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnHitComplete -= OnHitAnimationFinished;
    }

    private void OnHitAnimationFinished()
    {
        brain.ChangeState(_resumeState);
    }
}