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
}
