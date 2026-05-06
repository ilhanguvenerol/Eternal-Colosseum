using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates which enemy attacks when, using the same coroutine-based
/// turn-taking loop from the Arkham reference — adapted for EnemyBrain.
/// Also assigns Guard duty at fight start when ranged enemies are present.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minTurnDelay = 0.5f;
    [SerializeField] private float maxTurnDelay = 1.5f;
    [SerializeField] private float postRetreatDelay = 0.4f;
    [SerializeField] private float attackDuration = 1.5f;
    // All enemies registered under this manager
    private List<EnemyBrain> _all = new List<EnemyBrain>();

    // Subset available to attack (excludes guards and unavailable enemies)
    private List<EnemyBrain> _available = new List<EnemyBrain>();

    private void Start()
    {
        // Start is intentionally empty.
        // InitialiseWithEnemies is called by SpawnManager after enemies are spawned.
    }

    /// <summary>
    /// Called by SpawnManager once all enemies for this wave are instantiated.
    /// Replaces the old GetComponentsInChildren approach.
    /// </summary>
    public void InitialiseWithEnemies(System.Collections.Generic.List<EnemyBrain> enemies, UnityEngine.Transform player)
    {
        _all.Clear();
        _all.AddRange(enemies);

        AssignGuards();
        StartCoroutine(AI_Loop(null));
    }

    // ── Guard assignment ──────────────────────────────────────────────────────

    /// <summary>
    /// For each ranged enemy, find the nearest idle melee enemy and assign
    /// it as a guard. Guards are removed from the attack pool.
    /// </summary>
    private void AssignGuards()
    {
        foreach (EnemyBrain ranged in _all)
        {
            if (ranged.enemyType != EnemyType.Ranged) continue;

            EnemyBrain closestMelee = null;
            float      closestDist  = float.MaxValue;

            foreach (EnemyBrain melee in _all)
            {
                if (melee.enemyType != EnemyType.Melee) continue;
                if (melee.IsGuarding()) continue;

                float d = Vector3.Distance(melee.transform.position, ranged.transform.position);
                if (d < closestDist)
                {
                    closestDist  = d;
                    closestMelee = melee;
                }
            }

            if (closestMelee != null)
                closestMelee.AssignGuard(ranged);
        }
    }

    // ── AI loop ───────────────────────────────────────────────────────────────

    private IEnumerator AI_Loop(EnemyBrain lastAttacker)
    {
        if (AliveCount() == 0)
            yield break;

        yield return new WaitForSeconds(Random.Range(minTurnDelay, maxTurnDelay));

        // Pick attacker, preferring someone other than the last one
        EnemyBrain attacker = PickAttacker(exclude: lastAttacker)
                           ?? PickAttacker(exclude: null);

        if (attacker == null)
            yield break;

        // Wait until the chosen enemy is actually ready to move
        yield return new WaitUntil(() =>
            !attacker.IsRetreating() &&
            !attacker.IsStunned()    &&
            !attacker.IsGuarding());

        attacker.BeginAttackApproach();

        // OPTION B — fixed timeout. Replace with Option A once attack animations are ready.
        yield return new WaitForSeconds(attackDuration);

        // OPTION A — attack component drives retreat. Uncomment when ready:
        // yield return new WaitUntil(() => !attacker.IsEngaging());
        // (also remove the attackDuration field and WaitForSeconds above)


        attacker.BeginRetreat();

        yield return new WaitForSeconds(postRetreatDelay);

        if (AliveCount() > 0)
            StartCoroutine(AI_Loop(attacker));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Pick a random available melee attacker, optionally excluding one.
    /// Guards and ranged enemies are never picked here.
    /// </summary>
    private EnemyBrain PickAttacker(EnemyBrain exclude)
    {
        _available.Clear();

        foreach (EnemyBrain b in _all)
        {
            if (!b.isActiveAndEnabled)         continue; // dead
            if (b.enemyType == EnemyType.Ranged) continue; // ranged manage themselves
            if (b.IsGuarding())                continue; // guards don't leave position
            if (b == exclude)                  continue;

            _available.Add(b);
        }

        if (_available.Count == 0)
            return null;

        return _available[Random.Range(0, _available.Count)];
    }

    private int AliveCount()
    {
        int count = 0;
        foreach (EnemyBrain b in _all)
            if (b.isActiveAndEnabled) count++;
        return count;
    }

    /// <summary>
    /// Called by EnemyBrain (or an attack component) when an enemy dies,
    /// so its guard assignment can be cleaned up and a new guard found if needed.
    /// </summary>
    public void OnEnemyDied(EnemyBrain dead)
    {
        _all.Remove(dead);

        // If a ranged enemy died, release its guard back into the attack pool
        if (dead.enemyType == EnemyType.Ranged)
        {
            foreach (EnemyBrain b in _all)
            {
                if (b.IsGuarding() && b.guardTarget == dead)
                {
                    b.guardTarget = null;
                    b.ChangeState(new MeleeIdleState(b));
                }
            }
        }
    }
}
