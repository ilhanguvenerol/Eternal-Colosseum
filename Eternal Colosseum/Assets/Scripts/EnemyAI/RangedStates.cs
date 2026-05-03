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
    public LooseState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.StopMoving();
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

        // Stand still and fire — attack component reads this state via IsInLoose()
    }

    private bool HasGuard()
    {
        // Check if this ranged enemy has an active guard assigned to it.
        // guardTarget on the guard points back to this brain.
        EnemyBrain[] all = Object.FindObjectsOfType<EnemyBrain>();
        foreach (EnemyBrain e in all)
        {
            if (e.IsGuarding() && e.guardTarget == brain)
                return true;
        }
        return false;
    }
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

    private bool HasGuard()
    {
        EnemyBrain[] all = Object.FindObjectsOfType<EnemyBrain>();
        foreach (EnemyBrain e in all)
        {
            if (e.IsGuarding() && e.guardTarget == brain)
                return true;
        }
        return false;
    }
}

