using System.Collections.Generic;
using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    public class EnemySquadManager : MonoBehaviour
    {
        [Header("Spawn")]
        public int TotalEnemies = 6;
        [Range(0f, 1f)] public float RangedChance = 0.30f;
        [Range(0f, 1f)] public float CelestialChance = 0.15f;

        [Header("Prefabs")]
        public GameObject MeleePrefab;
        public GameObject RangedPrefab;

        [Header("Spawn Points")]
        public Transform[] SpawnPoints;
        public PlayerTargetPoints TargetPoints;

        [Header("Support Ring")]
        [Tooltip("Radius of the continuous ring where unassigned melee enemies wait.")]
        public float SupportRadius = 7f;

        [Tooltip("How strongly support enemies repel each other along the ring. Higher = wider spread.")]
        public float RepulsionStrength = 2f;

        [Tooltip("Minimum angular separation (degrees) before repulsion kicks in.")]
        public float MinSeparationDeg = 30f;

        [Tooltip("How fast (radians/sec) each enemy's angle slides toward its desired angle.")]
        public float AngleSlideSpeed = 1.5f;

        [Tooltip("Enemy only moves if its ring position shifted more than this distance.")]
        public float MoveThreshold = 0.3f;

        [Header("Assignment Stability")]
        [Tooltip("A new engage point must be this much closer (squared) before an enemy abandons its current one.")]
        public float ReassignmentThreshold = 4f;

        // ── References ───────────────────────────────────────────────────────
        PlayerTargetPoints _targetPoints;

        // ── Tracking ─────────────────────────────────────────────────────────
        readonly List<EnemyController> _allMelee = new();
        readonly List<EnemyController> _allRanged = new();

        readonly Dictionary<int, EnemyController> _engageAssignments = new();

        // Continuous ring: each support enemy owns a current angle (radians)
        readonly Dictionary<EnemyController, float> _supportAngles = new();

        // ── Unity ─────────────────────────────────────────────────────────────
        void Start()
        {
            SpawnWave(TargetPoints);
        }
        void Update()
        {
            if (_targetPoints == null) return;

            RecalculateEngagements();
            UpdateSupportRing();
        }

        // ── Spawn ─────────────────────────────────────────────────────────────
        public void SpawnWave(PlayerTargetPoints targetPoints)
        {
            _targetPoints = targetPoints;
            _allMelee.Clear();
            _allRanged.Clear();
            _engageAssignments.Clear();
            _supportAngles.Clear();

            for (int i = 0; i < TotalEnemies; i++)
            {
                bool isRanged = Random.value < RangedChance;
                bool isCelestial = Random.value < CelestialChance;

                Transform spawnPt = SpawnPoints[i % SpawnPoints.Length];
                GameObject prefab = isRanged ? RangedPrefab : MeleePrefab;

                EnemyController ec = Instantiate(prefab, spawnPt.position, spawnPt.rotation)
                                        .GetComponent<EnemyController>();
                ec.IsCelestial = isCelestial;
                ec.Squad = this;
                ec.TargetPoints = targetPoints;

                if (isRanged) _allRanged.Add(ec);
                else _allMelee.Add(ec);
            }

            AssignGuards();
            RecalculateEngagements();
        }

        // ── Engagement assignment ─────────────────────────────────────────────
        void RecalculateEngagements()
        {
            // Build reverse lookup: enemy → currently held point
            Dictionary<EnemyController, int> held = new();
            foreach (var kv in _engageAssignments)
                held[kv.Value] = kv.Key;

            List<EnemyController> unguarded = new();
            foreach (EnemyController e in _allMelee)
            {
                if (e == null || e.CurrentState is MeleeGuardState) continue;
                unguarded.Add(e);
            }

            // Nearest to any point gets first pick
            unguarded.Sort((a, b) =>
                SqrDistToClosestPoint(a.transform.position)
                    .CompareTo(SqrDistToClosestPoint(b.transform.position)));

            _engageAssignments.Clear();

            foreach (EnemyController e in unguarded)
            {
                int currentPoint = held.TryGetValue(e, out int cp) ? cp : -1;
                float currentDist = currentPoint >= 0
                    ? Vector3.SqrMagnitude(_targetPoints.GetPosition(currentPoint) - e.transform.position)
                    : float.MaxValue;

                // Find best free point
                int bestPoint = -1;
                float bestDist = float.MaxValue;
                for (int i = 0; i < _targetPoints.PointCount; i++)
                {
                    if (_engageAssignments.ContainsKey(i)) continue;
                    float d = Vector3.SqrMagnitude(_targetPoints.GetPosition(i) - e.transform.position);
                    if (d < bestDist) { bestDist = d; bestPoint = i; }
                }

                if (bestPoint < 0)
                {
                    AddToSupport(e);
                    continue;
                }

                bool currentStillFree = currentPoint >= 0
                    && !_engageAssignments.ContainsKey(currentPoint);

                int chosenPoint = (currentStillFree && bestDist >= currentDist - ReassignmentThreshold)
                    ? currentPoint
                    : bestPoint;

                AssignEngagePoint(e, chosenPoint);
                RemoveFromSupport(e);
            }
        }

        void AssignEngagePoint(EnemyController e, int pointIndex)
        {
            _engageAssignments[pointIndex] = e;
            bool isClosest = pointIndex == _targetPoints.GetClosestPointIndex(e.transform.position);
            e.AssignEngagePoint(pointIndex, isClosest);
        }

        float SqrDistToClosestPoint(Vector3 pos)
        {
            float best = float.MaxValue;
            for (int i = 0; i < _targetPoints.PointCount; i++)
            {
                float d = Vector3.SqrMagnitude(_targetPoints.GetPosition(i) - pos);
                if (d < best) best = d;
            }
            return best;
        }

        // ── Continuous support ring ───────────────────────────────────────────
        // No slots. Each enemy owns an angle on the ring and slides toward
        // its desired angle each frame. Desired angle = current angle pushed
        // away from neighbours by a repulsion force. Enemies naturally drift
        // to even spacing without any redistribution pass.
        void AddToSupport(EnemyController e)
        {
            if (!_supportAngles.ContainsKey(e))
            {
                // Initial angle: project the enemy's current world position onto the ring
                Vector3 toEnemy = e.transform.position - _targetPoints.transform.position;
                toEnemy.y = 0f;
                float initialAngle = toEnemy == Vector3.zero
                    ? 0f
                    : Mathf.Atan2(toEnemy.z, toEnemy.x);

                _supportAngles[e] = initialAngle;
            }

            if (e.CurrentState is not MeleeSupportState)
                e.GoMeleeSupport();
        }

        void RemoveFromSupport(EnemyController e)
        {
            _supportAngles.Remove(e);
        }

        void UpdateSupportRing()
        {
            if (_supportAngles.Count == 0) return;

            Vector3 playerPos = _targetPoints.transform.position;
            float minSepRad = MinSeparationDeg * Mathf.Deg2Rad;
            var enemies = new List<EnemyController>(_supportAngles.Keys);

            foreach (EnemyController e in enemies)
            {
                if (e == null) continue;

                float angle = _supportAngles[e];
                float totalPush = 0f;

                // Accumulate repulsion from every other support enemy
                foreach (EnemyController other in enemies)
                {
                    if (other == e || other == null) continue;

                    float otherAngle = _supportAngles[other];

                    // Shortest angular difference on the circle
                    float diff = Mathf.DeltaAngle(
                        angle * Mathf.Rad2Deg,
                        otherAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

                    float absDiff = Mathf.Abs(diff);

                    if (absDiff < minSepRad && absDiff > 0.001f)
                    {
                        // Repel proportionally to how close they are
                        float strength = (1f - absDiff / minSepRad) * RepulsionStrength;
                        // Push away: negative diff means other is behind us, so push forward
                        totalPush -= Mathf.Sign(diff) * strength * Time.deltaTime;
                    }
                }

                // Slide the angle
                angle += totalPush;

                // Keep in [0, 2π]
                angle = (angle % (2f * Mathf.PI) + 2f * Mathf.PI) % (2f * Mathf.PI);
                _supportAngles[e] = angle;

                // Compute world target
                Vector3 desiredPos = playerPos
                    + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * SupportRadius;

                // Only issue SetDestination when meaningfully displaced — avoids replanning jitter
                if (Vector3.SqrMagnitude(e.SupportTargetPosition - desiredPos) > MoveThreshold * MoveThreshold)
                    e.SupportTargetPosition = desiredPos;
            }
        }

        // ── Guard assignment ──────────────────────────────────────────────────
        void AssignGuards()
        {
            int guardCount = Mathf.Min(_allRanged.Count, _allMelee.Count);
            for (int i = 0; i < guardCount; i++)
            {
                EnemyController guard = _allMelee[i];
                EnemyController archer = _allRanged[i];

                guard.GuardTarget = archer;
                archer.AssignedGuard = guard;
                guard.GoMeleeGuard();
            }
        }

        // ── Public queries ────────────────────────────────────────────────────
        public Vector3 GetEngagePointPosition(int index)
            => _targetPoints != null ? _targetPoints.GetPosition(index) : Vector3.zero;

        // ── Cleanup ───────────────────────────────────────────────────────────
        public void OnEnemyDied(EnemyController dead)
        {
            _allMelee.Remove(dead);
            _allRanged.Remove(dead);
            RemoveFromSupport(dead);

            int heldPoint = -1;
            foreach (var kv in _engageAssignments)
                if (kv.Value == dead) { heldPoint = kv.Key; break; }
            if (heldPoint >= 0)
                _engageAssignments.Remove(heldPoint);

            if (dead.GuardTarget != null)
                dead.GuardTarget.AssignedGuard = null;
        }
    }
}