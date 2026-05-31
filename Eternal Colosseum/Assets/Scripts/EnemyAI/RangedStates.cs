using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// RANGED ENGAGE
// Moves toward the player until within firing range, then switches to Loose.
// ─────────────────────────────────────────────────────────────────────────────
public class RangedEngageState : EnemyState
{
    public RangedEngageState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.MoveTo(player.position, brain.engageSpeed);
    }

    public override void Update()
    {
        if (brain.DistanceToPlayer() <= brain.rangedFireDistance)
        {
            brain.ChangeState(new LooseState(brain));
            return;
        }

        // Keep destination current as the player moves
        brain.MoveTo(player.position, brain.engageSpeed);
    }

    public override void Exit()
    {
        brain.StopMoving();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LOOSE
// Stands ground and fires. Attack logic lives outside this file.
// Transitions to Disengage if the player gets too close AND a Guard is present.
// Transitions back to Engage if the player moves too far away.
// ─────────────────────────────────────────────────────────────────────────────
public class LooseState : EnemyState
{
    private const float DrawTime = 1.8f; // seconds to hold draw before releasing

    private enum FirePhase { Idle, Drawing, Loosing }
    private FirePhase _phase;

    public LooseState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.StopMoving();

        brain.EnemyAnimator.OnLoose += OnArrowReleased;
        brain.EnemyAnimator.OnLooseComplete += OnLooseAnimationComplete;

        BeginDraw();

    }

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        // Player moved out of range — give chase
        if (dist > brain.rangedFireDistance * 1.2f)
        {
            brain.ChangeState(new RangedEngageState(brain));
            return;
        }

        // Player too close — flee only if a guard is covering the retreat
        if (dist < brain.disengageThreshold && HasGuard())
        {
            brain.ChangeState(new DisengageState(brain));
            return;
        }

    }
    public override void Exit()
    {
        brain.EnemyAnimator.OnLoose -= OnArrowReleased;
        brain.EnemyAnimator.OnLooseComplete -= OnLooseAnimationComplete;
        brain.StopMoving();
    }

    private void BeginDraw()
    {
        _phase = FirePhase.Drawing;
        brain.EnemyAnimator.PlayDrawBow();
        // Wait DrawTime then release — use a coroutine on the brain
        brain.StartCoroutine(ReleaseAfterDraw());
    }

    private System.Collections.IEnumerator ReleaseAfterDraw()
    {
        yield return new WaitForSeconds(DrawTime);

        // Only release if still in this state — guard against state change mid-draw
        if (_phase != FirePhase.Drawing) yield break;

        _phase = FirePhase.Loosing;
        brain.EnemyAnimator.PlayLoose();
    }

    private void OnArrowReleased()//for whomever will handle damage connection
    {
        // Arrow spawning logic hooks here from a separate attack component.
        // LooseState does not own damage — it only drives the animation cycle.
    }

    private void OnLooseAnimationComplete()
    {
        if (_phase != FirePhase.Loosing) return;

        // Loose clip finished — wait fireRate seconds then draw again
        brain.StartCoroutine(WaitThenDraw());
    }

    private System.Collections.IEnumerator WaitThenDraw()
    {
        yield return new WaitForSeconds(brain.fireRate);

        // Only redraw if still in this state
        if (brain.Phase != EnemyPhase.Idle) yield break;

        BeginDraw();
    }

    private bool HasGuard() => brain.HasGuardAssigned();
}

// ─────────────────────────────────────────────────────────────────────────────
// DISENGAGE
// Runs directly away from the player until outside the disengage threshold
// plus a small buffer, then returns to Loose to resume firing.
// Only reached when a Guard is covering the retreat (checked in LooseState).
// ─────────────────────────────────────────────────────────────────────────────
public class DisengageState : EnemyState
{
    private float _safeDistance;

    public DisengageState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        _safeDistance = brain.disengageThreshold + 2f;
        SetFleeDestination();
    }

    public override void Update()
    {
        // Guard was lost mid-flee — stop running and face the player
        if (!HasGuard())
        {
            brain.ChangeState(new LooseState(brain));
            return;
        }

        if (brain.DistanceToPlayer() >= _safeDistance)
        {
            brain.ChangeState(new LooseState(brain));
            return;
        }

        // Refresh flee destination each frame to track a moving player
        SetFleeDestination();
    }

    public override void Exit()
    {
        brain.StopMoving();
    }

    private void SetFleeDestination()
    {
        Vector3 awayDir = (brain.transform.position - player.position).normalized;
        brain.MoveTo(brain.transform.position + awayDir * _safeDistance, brain.disengageSpeed);
    }

    private bool HasGuard() => brain.HasGuardAssigned();
}