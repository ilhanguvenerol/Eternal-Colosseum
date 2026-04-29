using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — ENGAGE
    // Moves directly toward the player.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeEngageState : IEnemyState
    {
        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Engage");
        }

        public void Execute(EnemyController e)
        {
            if (e.Player == null) return;

            e.Agent.SetDestination(e.Player.position);

            // Close enough to attack — handled by a separate AttackState or combat system
            // No automatic transition here; the combat system will call GoMeleeFlank /
            // GoMeleeSupport externally to vary behaviour after each attack.
            if (e.PlayerInRange(e.AttackRange))
            {
                // Hook: trigger your attack animation / damage here before transitioning
                e.Animator?.SetTrigger("Attack");
                e.DecidePostReachTransition();
            }

        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — FLANK
    // Orbits laterally around the player for FlankArcAngle degrees,
    // then transitions — mostly to Engage, occasionally to Support.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeFlankState : IEnemyState
    {
        float _arcTravelled;
        float _direction;  // +1 or -1 (clockwise / counter-clockwise)

        public void Enter(EnemyController e)
        {
            _arcTravelled = 0f;
            _direction    = Random.value > 0.5f ? 1f : -1f;
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Flank");
        }

        public void Execute(EnemyController e)
        {
            if (e.Player == null) return;

            // Orbit point: maintain SupportRadius distance while strafing
            Vector3 toEnemy = (e.transform.position - e.Player.position).normalized;
            Vector3 right   = Vector3.Cross(Vector3.up, toEnemy) * _direction;
            Vector3 target  = e.Player.position
                              + toEnemy * e.SupportRadius
                              + right   * 1.5f;   // lateral step size

            e.Agent.SetDestination(target);

            // Track how far around the arc we have moved
            float step = e.Agent.speed * Time.deltaTime * (180f / (Mathf.PI * e.SupportRadius));
            _arcTravelled += step;

            if (_arcTravelled >= e.FlankArcAngle)
            {
                bool goSupport = Random.value < e.FlankOddsSupport;
                if (goSupport) e.GoMeleeSupport();
                else           e.GoMeleeEngage();
            }
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — SUPPORT
    // Holds at SupportRadius from the player.
    // Transitions to Engage when:
    //   (a) signalled by another enemy (call NotifySwap())
    //   (b) the player walks within AttackRange of this unit
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeSupportState : IEnemyState
    {
        bool _swapSignalled;

        public void Enter(EnemyController e)
        {
            _swapSignalled    = false;
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Support");
        }

        public void Execute(EnemyController e)
        {
            if (e.Player == null) return;

            float dist = e.DistanceToPlayer();

            // Player walked into this unit's range — engage immediately
            if (dist <= e.AttackRange)
            {
                e.GoMeleeEngage();
                return;
            }

            // Another enemy requested a swap
            if (_swapSignalled)
            {
                e.GoMeleeEngage();
                return;
            }

            // Orbit at support radius
            Vector3 dir    = (e.transform.position - e.Player.position).normalized;
            Vector3 orbitPos = e.Player.position + dir * e.SupportRadius;
            e.Agent.SetDestination(orbitPos);
        }

        public void Exit(EnemyController e)
        {
            _swapSignalled    = false;
            e.Agent.isStopped = true;
        }

        // Called by an external coordinator (e.g. EnemySquadManager) to trigger swap
        public void NotifySwap() => _swapSignalled = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — GUARD
    // Stays between the player and a designated ranged ally.
    // GuardTarget must be set on the EnemyController before entering this state.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeGuardState : IEnemyState
    {
        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Guard");
        }

        public void Execute(EnemyController e)
        {
            if (e.Player == null || e.GuardTarget == null)
            {
                e.GoMeleeEngage();   // lost assignment — fall back
                return;
            }

            // Position on the line between player and ranged ally, closer to the player
            Vector3 toArcher = (e.GuardTarget.transform.position - e.Player.position).normalized;
            Vector3 guardPos  = e.Player.position + toArcher * (e.AttackRange * 1.2f);

            e.Agent.SetDestination(guardPos);
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }
}
