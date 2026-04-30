using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    // ─────────────────────────────────────────────────────────────────────────
    // RANGED — ENGAGE
    // Closes the gap until the player is within AttackRange.
    // ─────────────────────────────────────────────────────────────────────────
    public class RangedEngageState : IEnemyState
    {
        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Engage");
        }

        public void Execute(EnemyController e)
        {
            if (e.TargetPoints == null) return;

            if (e.PlayerInRange(e.AttackRange))
            {
                e.GoRangedLoose();
                return;
            }

            e.Agent.SetDestination(e.TargetPoints.PlayerTransform.position);
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RANGED — LOOSE
    // Stands ground and fires at the player.
    // Transitions to Disengage if player is too close AND a Guard is present.
    // ─────────────────────────────────────────────────────────────────────────
    public class RangedLooseState : IEnemyState
    {
        float _fireTimer;
        const float FireInterval = 1.5f;   // seconds between shots

        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = true;
            _fireTimer = 0f;
            e.Animator?.SetTrigger("Loose");
        }

        public void Execute(EnemyController e)
        {
            if (e.TargetPoints == null) return;

            // Face the player
            Vector3 look = e.TargetPoints.PlayerTransform.position - e.transform.position;
            look.y = 0f;
            if (look != Vector3.zero)
                e.transform.rotation = Quaternion.LookRotation(look);

            // Player closed in — try to disengage
            if (e.DistanceToPlayer() < e.MeleeThreshold)
            {
                if (e.HasGuard())
                {
                    e.GoRangedDisengage();
                    return;
                }
                // No guard present — stay in Loose and keep firing (archer must fend off)
            }

            // Fire
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= FireInterval)
            {
                _fireTimer = 0f;
                Fire(e);
            }
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = false;
        }

        void Fire(EnemyController e)
        {
            // Hook into your own projectile / attack system here.
            // Example: ProjectilePool.Instance.Shoot(e.transform, e.Player);
            e.Animator?.SetTrigger("Fire");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RANGED — DISENGAGE
    // Runs away from the player until a safe distance is re-established.
    // Requires a Guard melee ally. If the guard is lost mid-flee, falls back to Loose.
    // ─────────────────────────────────────────────────────────────────────────
    public class RangedDisengageState : IEnemyState
    {
        float _safeDistance;

        public void Enter(EnemyController e)
        {
            _safeDistance     = e.AttackRange + 3f;   // re-engage from here
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Disengage");
        }

        public void Execute(EnemyController e)
        {
            if (e.TargetPoints == null) return;

            // Guard was lost — stop fleeing
            if (!e.HasGuard())
            {
                e.GoRangedLoose();
                return;
            }

            float dist = e.DistanceToPlayer();

            if (dist >= _safeDistance)
            {
                // Safe again — resume firing
                e.GoRangedLoose();
                return;
            }

            // Move directly away from player
            Vector3 fleeDir = (e.transform.position - e.TargetPoints.PlayerTransform.position).normalized;
            e.Agent.SetDestination(e.transform.position + fleeDir * 4f);
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }
}
