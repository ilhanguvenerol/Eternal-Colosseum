using System.Collections.Generic;
using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    // Manages a single arena wave: assigns guards to ranged enemies
    // and coordinates support-swap signals between melee units.
    public class EnemySquadManager : MonoBehaviour
    {
        [Header("Spawn")]
        public int  TotalEnemies   = 6;
        [Range(0f, 1f)] public float RangedChance   = 0.30f;
        [Range(0f, 1f)] public float CelestialChance = 0.15f;

        [Header("Prefabs")]
        public GameObject MeleePrefab;
        public GameObject RangedPrefab;

        [Header("Spawn points")]
        public Transform[] SpawnPoints;

        [Header("Engagement")]
        [Tooltip("Maximum number of melee enemies allowed to Engage simultaneously.")]
        public int MaxEngagingEnemies = 2;

        readonly List<EnemyController> _meleeUnits  = new();
        readonly List<EnemyController> _rangedUnits = new();

        // ── Engaging count ────────────────────────────────────────────────────
        public int EngagingCount
        {
            get
            {
                int count = 0;
                foreach (EnemyController m in _meleeUnits)
                    if (m.CurrentState is MeleeEngageState) count++;
                return count;
            }
        }

        public bool CanEngage => EngagingCount < MaxEngagingEnemies;

        // ── Spawn ─────────────────────────────────────────────────────────────
        public void SpawnWave()
        {
            _meleeUnits.Clear();
            _rangedUnits.Clear();

            for (int i = 0; i < TotalEnemies; i++)
            {
                bool isRanged    = Random.value < RangedChance;
                bool isCelestial = Random.value < CelestialChance;

                Transform spawnPt = SpawnPoints[i % SpawnPoints.Length];
                GameObject prefab = isRanged ? RangedPrefab : MeleePrefab;

                GameObject obj = Instantiate(prefab, spawnPt.position, spawnPt.rotation);
                EnemyController ec = obj.GetComponent<EnemyController>();
                ec.IsCelestial = isCelestial;

                if (isRanged) _rangedUnits.Add(ec);
                else          _meleeUnits.Add(ec);
            }

            AssignGuards();
        }

        // ── Guard assignment ──────────────────────────────────────────────────
        // Each ranged enemy gets at most one melee guard.
        // Excess melee units remain unassigned (Engage / Flank / Support freely).
        void AssignGuards()
        {
            int guardCount = Mathf.Min(_rangedUnits.Count, _meleeUnits.Count);

            for (int i = 0; i < guardCount; i++)
            {
                EnemyController guard  = _meleeUnits[i];
                EnemyController archer = _rangedUnits[i];

                guard.GuardTarget    = archer;
                archer.AssignedGuard = guard;

                guard.GoMeleeGuard();
            }
        }

        // ── Support swap ──────────────────────────────────────────────────────
        // Call this from your combat system when an engaging melee unit
        // wants to rotate out (e.g. after performing an attack).
        public void RequestSupportSwap(EnemyController engagingUnit)
        {
            // Find an idle support unit and signal it to take over
            foreach (EnemyController m in _meleeUnits)
            {
                if (m == engagingUnit) continue;
                if (m.CurrentState is MeleeSupportState support)
                {
                    support.NotifySwap();
                    engagingUnit.GoMeleeSupport();  // rotating unit becomes support
                    return;
                }
            }
            // No support unit available — engaging unit stays engaged
        }

        // ── Cleanup ───────────────────────────────────────────────────────────
        public void OnEnemyDied(EnemyController dead)
        {
            _meleeUnits.Remove(dead);
            _rangedUnits.Remove(dead);

            // If this was a guard, the paired archer loses its guard reference
            if (dead.GuardTarget != null)
                dead.GuardTarget.AssignedGuard = null;
        }

        private void Start() // this is a test, may be removable
        {
            SpawnWave();
        }
    }
}
