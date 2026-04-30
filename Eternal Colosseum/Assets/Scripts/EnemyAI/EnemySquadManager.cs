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
        [Tooltip("Radius of the outer ring where unassigned melee enemies wait.")]
        public float SupportRadius = 7f;

        // ── References ────────────────────────────────────────────────────────
        PlayerTargetPoints _targetPoints;

        // ── Tracking ──────────────────────────────────────────────────────────
        readonly List<EnemyController> _allMelee = new();
        readonly List<EnemyController> _allRanged = new();

        // pointIndex → assigned enemy  (-1 key means unassigned)
        readonly Dictionary<int, EnemyController> _engageAssignments = new();

        // support enemy → their support ring position index
        readonly Dictionary<EnemyController, int> _supportPositions = new();

        // World-space support ring positions (recalculated when group changes)
        Vector3[] _supportRingPositions = System.Array.Empty<Vector3>();

        // ── Unity ─────────────────────────────────────────────────────────────
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
            _supportPositions.Clear();

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
        // Every frame: for each melee enemy, check whether a closer free point
        // exists. If so, reassign. Enemies without a point go to Support ring.
        void RecalculateEngagements()
        {
            // Build reverse lookup: enemy → currently assigned point index
            Dictionary<EnemyController, int> currentAssignment = new();
            foreach (var kv in _engageAssignments)
                currentAssignment[kv.Value] = kv.Key;

            // Release all assignments — we'll reassign cleanly each tick.
            // This is safe because assignment is O(n*m) and n is small.
            _engageAssignments.Clear();

            // Sort melee enemies by distance to their closest available point
            // so nearer enemies get priority.
            List<EnemyController> unguarded = new();
            foreach (EnemyController e in _allMelee)
            {
                if (e == null || e.CurrentState is MeleeGuardState) continue;
                unguarded.Add(e);
            }

            unguarded.Sort((a, b) =>
            {
                int ia = _targetPoints.GetClosestPointIndex(a.transform.position);
                int ib = _targetPoints.GetClosestPointIndex(b.transform.position);
                float da = Vector3.SqrMagnitude(_targetPoints.GetPosition(ia) - a.transform.position);
                float db = Vector3.SqrMagnitude(_targetPoints.GetPosition(ib) - b.transform.position);
                return da.CompareTo(db);
            });

            bool supportChanged = false;

            foreach (EnemyController e in unguarded)
            {
                // Find closest point not yet taken
                int point = GetClosestFreePoint(e.transform.position);
                if (point >= 0)
                {
                    _engageAssignments[point] = e;

                    bool wasInSupport = _supportPositions.ContainsKey(e);

                    bool isClosestPoint = point == _targetPoints.GetClosestPointIndex(e.transform.position);
                    e.AssignEngagePoint(point, isClosestPoint);

                    if (wasInSupport)
                    {
                        _supportPositions.Remove(e);
                        supportChanged = true;
                    }

                    continue;
                }

                // No free point — enemy goes to support ring
                if (!_supportPositions.ContainsKey(e))
                {
                    _supportPositions[e] = -1;  // index assigned below
                    supportChanged = true;
                }

                if (e.CurrentState is not MeleeSupportState)
                    e.GoMeleeSupport();
            }

            if (supportChanged)
                RebuildSupportRing();
        }

        int GetClosestFreePoint(Vector3 from)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _targetPoints.PointCount; i++)
            {
                if (_engageAssignments.ContainsKey(i)) continue;
                float d = Vector3.SqrMagnitude(_targetPoints.GetPosition(i) - from);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        // ── Support ring ──────────────────────────────────────────────────────
        void RebuildSupportRing()
        {
            List<EnemyController> supporters = new(_supportPositions.Keys);
            int count = supporters.Count;

            if (count == 0)
            {
                _supportRingPositions = System.Array.Empty<Vector3>();
                return;
            }

            _supportRingPositions = new Vector3[count];

            float angleStep = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                _supportRingPositions[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * SupportRadius;
                // Stored as offset from player — added to player position in UpdateSupportRing
                _supportPositions[supporters[i]] = i;
            }
        }

        void UpdateSupportRing()
        {
            if (_targetPoints == null || _supportRingPositions.Length == 0) return;

            Vector3 playerPos = _targetPoints.transform.position;
            foreach (var kv in _supportPositions)
            {
                EnemyController e = kv.Key;
                int idx = kv.Value;
                if (e == null || idx < 0 || idx >= _supportRingPositions.Length) continue;

                e.SupportTargetPosition = playerPos + _supportRingPositions[idx];
            }
        }

        // ── Guard assignment ───────────────────────────────────────────────────
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
            _supportPositions.Remove(dead);

            // Release engage point if this enemy held one
            int heldPoint = -1;
            foreach (var kv in _engageAssignments)
                if (kv.Value == dead) { heldPoint = kv.Key; break; }
            if (heldPoint >= 0)
                _engageAssignments.Remove(heldPoint);

            if (dead.GuardTarget != null)
                dead.GuardTarget.AssignedGuard = null;

            RebuildSupportRing();
        }
        private void Start()
        {
            SpawnWave(TargetPoints);
        }
    }
}