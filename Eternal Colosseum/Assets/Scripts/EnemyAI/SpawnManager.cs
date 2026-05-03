using System.Collections.Generic;
using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    /// <summary>
    /// Responsible for one thing: spawning the correct enemies for a given
    /// level and stage, then handing them to EnemyManager.
    ///
    /// Deliberately separate from EnemyManager so scaling, prefabs, and
    /// spawn points can be tweaked without touching AI orchestration.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Prefabs")]
        [Tooltip("Standard melee enemy prefab. Must have EnemyBrain component.")]
        public GameObject MeleePrefab;

        [Tooltip("Standard ranged enemy prefab. Must have EnemyBrain component.")]
        public GameObject RangedPrefab;

        [Tooltip("Celestial melee variant. Falls back to MeleePrefab if null.")]
        public GameObject CelestialMeleePrefab;

        [Tooltip("Celestial ranged variant. Falls back to RangedPrefab if null.")]
        public GameObject CelestialRangedPrefab;

        [Header("Spawn Points")]
        [Tooltip("World positions enemies can spawn at. Cycled if count > points.")]
        public Transform[] SpawnPoints;

        [Header("Scaling")]
        public WaveScalingData Scaling;

        [Header("References")]
        public EnemyManager EnemyManager;
        public Transform    Player;

        // ── Public API ────────────────────────────────────────────────────────
        private void Start()
        {
            SpawnWave(5, 2);
        }
        /// <summary>
        /// Spawns a wave for the given level and stage.
        /// Call this from your level/fight controller when a fight begins.
        /// </summary>
        public void SpawnWave(int level, int stage)
        {
            if (!ValidateSetup()) return;

            WaveParameters p = Scaling.Calculate(level, stage);
            Debug.Log($"[SpawnManager] Level {level} Stage {stage} → {p}");

            List<EnemyBrain> spawned = new List<EnemyBrain>();

            for (int i = 0; i < p.EnemyCount; i++)
            {
                bool isCelestial = Random.value < p.CelestialChance;
                bool isRanged    = Random.value < p.RangedChance;

                GameObject prefab = PickPrefab(isRanged, isCelestial);
                Transform  spawnPt = SpawnPoints[i % SpawnPoints.Length];

                GameObject go = Instantiate(prefab, spawnPt.position, spawnPt.rotation, transform);
                EnemyBrain brain = go.GetComponent<EnemyBrain>();

                // Inject shared player reference
                brain.SetPlayer(Player);

                // Tag celestial for visual/stat system to read later
                brain.IsCelestial = isCelestial;

                spawned.Add(brain);
            }

            // Hand off to the AI orchestrator
            EnemyManager.InitialiseWithEnemies(spawned, Player);
        }

        // ── Private ───────────────────────────────────────────────────────────

        GameObject PickPrefab(bool isRanged, bool isCelestial)
        {
            if (isRanged)
                return (isCelestial && CelestialRangedPrefab != null)
                    ? CelestialRangedPrefab : RangedPrefab;

            return (isCelestial && CelestialMeleePrefab != null)
                ? CelestialMeleePrefab : MeleePrefab;
        }

        bool ValidateSetup()
        {
            if (MeleePrefab  == null) { Debug.LogError("[SpawnManager] MeleePrefab is not assigned.");  return false; }
            if (RangedPrefab == null) { Debug.LogError("[SpawnManager] RangedPrefab is not assigned."); return false; }
            if (SpawnPoints == null || SpawnPoints.Length == 0) { Debug.LogError("[SpawnManager] No spawn points assigned."); return false; }
            if (Scaling      == null) { Debug.LogError("[SpawnManager] WaveScalingData is not assigned."); return false; }
            if (EnemyManager == null) { Debug.LogError("[SpawnManager] EnemyManager is not assigned."); return false; }
            if (Player       == null) { Debug.LogError("[SpawnManager] Player is not assigned."); return false; }
            return true;
        }
    }
}
