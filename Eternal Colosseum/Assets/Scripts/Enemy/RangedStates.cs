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
            if (e.Player == null) return;

            if (e.PlayerInRange(e.AttackRange))
            {
                e.GoRangedLoose();
                return;
            }

            e.Agent.SetDestination(e.Player.position);
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
            if (e.Player == null) return;

            // Face the player
            Vector3 look = e.Player.position - e.transform.position;
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
}
