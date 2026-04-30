using UnityEngine;

namespace EternalColosseum.EnemyAI
{
    // Sits on the Player GameObject.
    // Maintains N evenly-spaced target points around the player at EngageRadius.
    // Points rotate with the player and update every frame.
    public class PlayerTargetPoints : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("How many engagement slots exist around the player.")]
        public int   PointCount   = 6;
        public float EngageRadius = 2.2f;   // should match or slightly exceed AttackRange

        // World-space positions, recalculated every frame
        public Vector3[] Positions { get; private set; }

        void Awake()
        {
            Positions = new Vector3[PointCount];
        }

        void Update()
        {
            UpdatePositions();
        }

        void UpdatePositions()
        {
            float angleStep = 360f / PointCount;
            for (int i = 0; i < PointCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Positions[i] = transform.position
                    + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * EngageRadius;
            }
        }

        // Returns the index of the point closest to a given world position.
        public int GetClosestPointIndex(Vector3 from)
        {
            int   best     = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < PointCount; i++)
            {
                float d = Vector3.SqrMagnitude(Positions[i] - from);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        // Returns the world position for a given index.
        public Vector3 GetPosition(int index) => Positions[index];

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (Positions == null) return;
            Gizmos.color = Color.yellow;
            foreach (Vector3 p in Positions)
                Gizmos.DrawWireSphere(p, 0.2f);
        }
#endif
    }
}
