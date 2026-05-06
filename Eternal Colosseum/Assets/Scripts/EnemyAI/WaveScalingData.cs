using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    /// <summary>
    /// Pure data: translates level + stage into concrete spawn parameters.
    ///
    /// GDD rule:
    ///   "Use current level (1-16) as an additive,
    ///    current stage (1-4) as a multiplier to determine:
    ///    Enemy count, Ranged enemy chance, Celestial enemy chance."
    ///
    /// Interpretation:
    ///   base value + level  (additive)
    ///   then × stage        (multiplier)
    ///   clamped to designer-set min/max.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveScaling", menuName = "EternalColosseum/WaveScaling")]
    public class WaveScalingData : ScriptableObject
    {
        [Header("Enemy Count")]
        [Tooltip("Flat count added before level scaling.")]
        public int   EnemyCountBase    = 2;
        [Tooltip("Added per level (1-16).")]
        public float EnemyCountPerLevel = 0.25f;
        public int   EnemyCountMin     = 2;
        public int   EnemyCountMax     = 12;

        [Header("Ranged Chance  (0-1)")]
        public float RangedChanceBase     = 0.05f;
        public float RangedChancePerLevel = 0.015f;
        public float RangedChanceMin      = 0f;
        public float RangedChanceMax      = 0.45f;

        [Header("Celestial Chance  (0-1)")]
        public float CelestialChanceBase     = 0.00f;
        public float CelestialChancePerLevel = 0.01f;
        public float CelestialChanceMin      = 0f;
        public float CelestialChanceMax      = 0.40f;

        // ── Formula ───────────────────────────────────────────────────────────

        /// <param name="level">1-16 within the full run</param>
        /// <param name="stage">1-4</param>
        public WaveParameters Calculate(int level, int stage)
        {
            float l = Mathf.Clamp(level, 1, 16);
            float s = Mathf.Clamp(stage, 1,  4);

            int enemyCount = Mathf.Clamp(
                Mathf.RoundToInt((EnemyCountBase + EnemyCountPerLevel * l) * s),
                EnemyCountMin, EnemyCountMax);

            float rangedChance = Mathf.Clamp(
                (RangedChanceBase + RangedChancePerLevel * l) * s,
                RangedChanceMin, RangedChanceMax);

            float celestialChance = Mathf.Clamp(
                (CelestialChanceBase + CelestialChancePerLevel * l) * s,
                CelestialChanceMin, CelestialChanceMax);

            return new WaveParameters(enemyCount, rangedChance, celestialChance);
        }
    }

    /// <summary>Resolved spawn parameters for one fight.</summary>
    public readonly struct WaveParameters
    {
        public readonly int   EnemyCount;
        public readonly float RangedChance;
        public readonly float CelestialChance;

        public WaveParameters(int count, float ranged, float celestial)
        {
            EnemyCount      = count;
            RangedChance    = ranged;
            CelestialChance = celestial;
        }

        public override string ToString()
            => $"Enemies:{EnemyCount}  Ranged:{RangedChance:P0}  Celestial:{CelestialChance:P0}";
    }
}
