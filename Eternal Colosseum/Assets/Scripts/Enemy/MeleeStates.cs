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
}
