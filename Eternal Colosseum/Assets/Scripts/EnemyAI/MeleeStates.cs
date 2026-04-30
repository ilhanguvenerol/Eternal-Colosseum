using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — ENGAGE
    // Moves toward the assigned target point.
    //
    // If the assigned point IS the closest point to this enemy:
    //   → Move in a straight line.
    // If the assigned point is NOT the closest:
    //   → Curve around the player (flank) to reach it.
    //
    // The squad manager handles reassignment each frame.
    // This state just follows whatever point it has been given.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeEngageState : IEnemyState
    {
        // Curve control: how far the arc midpoint is pushed perpendicular
        // to the direct path. Higher = wider curve.
        const float CurveStrength = 3.5f;
        const float ArrivalThreshold = 0.4f;

        // For curved movement we walk a bezier by advancing a t parameter
        float _t;
        Vector3 _curveStart;
        Vector3 _curveMid;
        Vector3 _curveEnd;
        bool _curving;
        int _lastAssignedPoint = -1;

        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Engage");
            RecalculatePath(e);
        }

        public void Execute(EnemyController e)
        {
            if (e.TargetPoints == null || e.AssignedPointIndex < 0)
                return;

            // If assignment changed, recalculate
            if (e.AssignedPointIndex != _lastAssignedPoint)
                RecalculatePath(e);

            if (_curving)
                ExecuteCurved(e);
            else
                ExecuteStraight(e);

            FacePlayer(e);
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
            _lastAssignedPoint = -1;
        }

        // ── Straight path ─────────────────────────────────────────────────────
        void ExecuteStraight(EnemyController e)
        {
            e.Agent.SetDestination(e.AssignedPointPosition);
        }

        // ── Curved path (flanking) ────────────────────────────────────────────
        // Walks a quadratic bezier: start → mid (offset perpendicularly) → end.
        // Each frame we advance t based on speed and sample the curve for the
        // NavMesh destination. This produces a smooth arc without extra states.
        void ExecuteCurved(EnemyController e)
        {
            // Advance t
            float arcLength = EstimateArcLength();
            _t += (e.Agent.speed * Time.deltaTime) / Mathf.Max(arcLength, 0.1f);
            _t = Mathf.Clamp01(_t);

            // Sample bezier
            Vector3 target = SampleBezier(_t);
            e.Agent.SetDestination(target);

            // Update curve end in case the target point moved (player moved)
            _curveEnd = e.AssignedPointPosition;

            // If we are close to the destination, let the squad manager
            // take over — it will recalculate on the next frame
        }

        void RecalculatePath(EnemyController e)
        {
            _lastAssignedPoint = e.AssignedPointIndex;
            _t = 0f;

            _curving = !e.IsClosestPoint;

            if (_curving)
            {
                _curveStart = e.transform.position;
                _curveEnd = e.AssignedPointPosition;

                // Midpoint: halfway along direct path, pushed sideways
                // relative to the player position to create an arc around them
                Vector3 mid = (_curveStart + _curveEnd) * 0.5f;
                Vector3 toMid = (mid - e.TargetPoints.transform.position).normalized;
                _curveMid = mid + toMid * CurveStrength;
            }
        }

        Vector3 SampleBezier(float t)
        {
            // Quadratic bezier: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            float u = 1f - t;
            return u * u * _curveStart
                 + 2f * u * t * _curveMid
                 + t * t * _curveEnd;
        }

        float EstimateArcLength()
        {
            // Approximate with a few samples — cheap enough for gameplay
            int samples = 10;
            float length = 0f;
            Vector3 prev = SampleBezier(0f);
            for (int i = 1; i <= samples; i++)
            {
                Vector3 curr = SampleBezier(i / (float)samples);
                length += Vector3.Distance(prev, curr);
                prev = curr;
            }
            return length;
        }

        void FacePlayer(EnemyController e)
        {
            if (e.TargetPoints == null) return;
            Vector3 look = e.TargetPoints.transform.position - e.transform.position;
            look.y = 0f;
            if (look != Vector3.zero)
                e.transform.rotation = Quaternion.RotateTowards(
                    e.transform.rotation,
                    Quaternion.LookRotation(look),
                    180f * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — SUPPORT
    // Holds position at SupportTargetPosition, set each frame by
    // EnemySquadManager. Enemies are distributed evenly on the outer ring.
    // Transitions to Engage automatically when the squad manager assigns a point.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeSupportState : IEnemyState
    {
        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Support");
        }

        public void Execute(EnemyController e)
        {
            // Squad manager calls AssignEngagePoint which transitions to Engage —
            // nothing to check here. Just hold the ring position.
            if (Vector3.SqrMagnitude(e.transform.position - e.SupportTargetPosition) > 2f)
                e.Agent.SetDestination(e.SupportTargetPosition);


            // Face the player
            if (e.TargetPoints != null)
            {
                Vector3 look = e.TargetPoints.transform.position - e.transform.position;
                look.y = 0f;
                if (look != Vector3.zero)
                    e.transform.rotation = Quaternion.RotateTowards(
                        e.transform.rotation,
                        Quaternion.LookRotation(look),
                        120f * Time.deltaTime);
            }
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE — GUARD
    // Stays close to its assigned ranged ally, between the ally and the player.
    // ─────────────────────────────────────────────────────────────────────────
    public class MeleeGuardState : IEnemyState
    {
        // How close to stay to the archer
        const float GuardOffset = 1.5f;

        public void Enter(EnemyController e)
        {
            e.Agent.isStopped = false;
            e.Animator?.SetTrigger("Guard");
        }

        public void Execute(EnemyController e)
        {
            if (e.GuardTarget == null || e.TargetPoints == null)
            {
                e.GoMeleeEngage();
                return;
            }

            // Stay between the archer and the player, close to the archer
            Vector3 archerPos = e.GuardTarget.transform.position;
            Vector3 playerPos = e.TargetPoints.transform.position;

            Vector3 archerToPlayer = (playerPos - archerPos).normalized;
            Vector3 guardPos = archerPos + archerToPlayer * GuardOffset;

            e.Agent.SetDestination(guardPos);

            // Face the player
            Vector3 look = playerPos - e.transform.position;
            look.y = 0f;
            if (look != Vector3.zero)
                e.transform.rotation = Quaternion.RotateTowards(
                    e.transform.rotation,
                    Quaternion.LookRotation(look),
                    120f * Time.deltaTime);
        }

        public void Exit(EnemyController e)
        {
            e.Agent.isStopped = true;
        }
    }
}