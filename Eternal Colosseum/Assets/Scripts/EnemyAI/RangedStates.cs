using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// RANGED ENGAGE
// Moves toward the player until within firing range, then switches to Loose.
// ─────────────────────────────────────────────────────────────────────────────
public class RangedEngageState : EnemyState
{
    public RangedEngageState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        if (dist > brain.rangedFireDistance)
        {
            Vector3 dir = (player.position - brain.transform.position).normalized;
            brain.Move(dir, brain.engageSpeed);
        }
        else
        {
            brain.ChangeState(new LooseState(brain));
        }
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

    public override void Update()
    {
        float dist = brain.DistanceToPlayer();

        // Player moved out of range: give chase
        if (dist > brain.rangedFireDistance * 1.2f)
        {
            brain.ChangeState(new RangedEngageState(brain));
            return;
        }

        // Player too close: flee only if a guard is nearby to cover the retreat
        if (dist < brain.disengageThreshold && HasNearbyGuard())
        {
            brain.ChangeState(new DisengageState(brain));
        }

        // Otherwise: stand still, fire. Attack component handles projectile.
    }

    private bool HasNearbyGuard()
    {
        // Find all EnemyBrains in scene and check for an active Guard
        // covering this specific ranged enemy
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
    // How far past the threshold to run before returning to Loose
    private const float SafeBuffer = 2f;

    public DisengageState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        float dist    = brain.DistanceToPlayer();
        float safeDistance = brain.disengageThreshold + SafeBuffer;

        if (dist < safeDistance)
        {
            // Run directly away from the player
            Vector3 awayDir = (brain.transform.position - player.position).normalized;
            brain.Move(awayDir, brain.disengageSpeed);
        }
        else
        {
            // Reached safety — return to firing position
            brain.ChangeState(new LooseState(brain));
        }
    }
}
