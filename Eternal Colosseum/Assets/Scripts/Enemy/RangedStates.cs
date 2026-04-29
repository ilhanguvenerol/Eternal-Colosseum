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
}
