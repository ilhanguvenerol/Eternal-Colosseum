using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates sequential melee turn-taking and guard assignment.
///
/// Reads brain.Phase instead of Is*() type checks.
/// Calls brain.ChangeState() directly — no wrapper entry-point methods.
///
/// All four loop bugs from the original are fixed:
///   1. Attacker dying mid-engage no longer calls ChangeState on a dead object.
///   2. WaitUntil readiness check escapes if the picked enemy dies while waiting.
///   3. Retreat is skipped when the enemy already self-transitioned out of Engage.
///   4. All-guards softlock: guards are released when no free attacker exists.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minTurnDelay = 0.5f;
    [SerializeField] private float maxTurnDelay = 1.5f;
    [SerializeField] private float postRetreatDelay = 0.4f;

    private List<EnemyBrain> _all = new List<EnemyBrain>();
    private List<EnemyBrain> _available = new List<EnemyBrain>();

    private void Start() { } // intentionally empty

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by SpawnManager once all enemies are instantiated.</summary>
    public void InitialiseWithEnemies(List<EnemyBrain> enemies, Transform player)
    {
        _all.Clear();
        _all.AddRange(enemies);

        foreach (EnemyBrain b in _all)
            b.EnemyManager = this;

        AssignGuards();
        StartCoroutine(AI_Loop(null));
    }

    /// <summary>
    /// Returns true if any alive melee enemy is currently guarding the given ranged brain.
    /// Called by ranged states via EnemyBrain.HasGuardAssigned().
    /// </summary>
    public bool HasGuardFor(EnemyBrain ranged)
    {
        foreach (EnemyBrain b in _all)
            if (b.isActiveAndEnabled && b.Phase == EnemyPhase.Guarding && b.guardTarget == ranged)
                return true;
        return false;
    }

    /// <summary>
    /// Called when an enemy dies. Cleans up guard assignments and releases
    /// any melee guard whose ranged charge just died.
    /// </summary>
    public void OnEnemyDied(EnemyBrain dead)
    {
        _all.Remove(dead);

        if (dead.enemyType == EnemyType.Ranged)
        {
            foreach (EnemyBrain b in _all)
            {
                if (b.Phase == EnemyPhase.Guarding && b.guardTarget == dead)
                {
                    b.guardTarget = null;
                    b.ChangeState(new MeleeIdleState(b));
                }
            }
        }
    }

    // ── Guard assignment ──────────────────────────────────────────────────────

    private void AssignGuards()
    {
        foreach (EnemyBrain ranged in _all)
        {
            if (ranged.enemyType != EnemyType.Ranged) continue;

            EnemyBrain closestMelee = null;
            float closestDist = float.MaxValue;

            foreach (EnemyBrain melee in _all)
            {
                if (melee.enemyType != EnemyType.Melee) continue;
                if (melee.Phase == EnemyPhase.Guarding) continue;

                float d = Vector3.Distance(melee.transform.position, ranged.transform.position);
                if (d < closestDist) { closestDist = d; closestMelee = melee; }
            }

            if (closestMelee != null)
            {
                closestMelee.guardTarget = ranged;
                closestMelee.ChangeState(new GuardState(closestMelee));
            }
        }
    }

    // ── AI loop ───────────────────────────────────────────────────────────────

    private IEnumerator AI_Loop(EnemyBrain lastAttacker)
    {
        if (AliveCount() == 0) yield break;

        yield return new WaitForSeconds(Random.Range(minTurnDelay, maxTurnDelay));

        EnemyBrain attacker = PickAttacker(exclude: lastAttacker)
                           ?? PickAttacker(exclude: null);

        // Bug fix 4 — no free attacker means all remaining melee are guards.
        // Release them so the fight doesn't softlock.
        if (attacker == null)
        {
            if (AliveCount() > 0) ReleaseAllGuards();
            yield break;
        }

        // Bug fix 2 — escape the wait if the chosen enemy dies before its turn.
        yield return new WaitUntil(() =>
            !attacker.isActiveAndEnabled ||
            (attacker.Phase != EnemyPhase.Retreating &&
             attacker.Phase != EnemyPhase.Stunned &&
             attacker.Phase != EnemyPhase.Guarding));

        if (!attacker.isActiveAndEnabled)
        {
            if (AliveCount() > 0) StartCoroutine(AI_Loop(null));
            yield break;
        }

        attacker.ChangeState(new MeleeEngageState(attacker));

        // Wait for MeleeEngageState to resolve (punch complete → transitions
        // itself to MeleeIdleState, so Phase leaves Engaging).
        yield return new WaitUntil(() =>
            !attacker.isActiveAndEnabled ||
            attacker.Phase != EnemyPhase.Engaging);

        // Bug fix 1 — skip retreat if attacker died during engage.
        if (!attacker.isActiveAndEnabled)
        {
            if (AliveCount() > 0) StartCoroutine(AI_Loop(null));
            yield break;
        }

        // Bug fix 3 — only retreat if still in Engaging phase.
        // If the enemy already self-transitioned (player fled, AbandonMultiplier),
        // it is already in Idle — no need to push it into Retreat.
        if (attacker.Phase == EnemyPhase.Engaging)
            attacker.ChangeState(new MeleeRetreatState(attacker));

        yield return new WaitForSeconds(postRetreatDelay);

        if (AliveCount() > 0)
            StartCoroutine(AI_Loop(attacker));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private EnemyBrain PickAttacker(EnemyBrain exclude)
    {
        _available.Clear();

        foreach (EnemyBrain b in _all)
        {
            if (!b.isActiveAndEnabled) continue;
            if (b.enemyType == EnemyType.Ranged) continue;
            if (b.Phase == EnemyPhase.Guarding) continue;
            if (b == exclude) continue;
            _available.Add(b);
        }

        return _available.Count == 0
            ? null
            : _available[Random.Range(0, _available.Count)];
    }

    private int AliveCount()
    {
        int count = 0;
        foreach (EnemyBrain b in _all)
            if (b.isActiveAndEnabled) count++;
        return count;
    }

    /// <summary>
    /// Releases all guarding enemies back to idle and restarts the loop.
    /// Called when PickAttacker returns null but enemies are still alive (bug fix 4).
    /// </summary>
    private void ReleaseAllGuards()
    {
        foreach (EnemyBrain b in _all)
        {
            if (!b.isActiveAndEnabled) continue;
            if (b.Phase != EnemyPhase.Guarding) continue;
            b.guardTarget = null;
            b.ChangeState(new MeleeIdleState(b));
        }

        if (AliveCount() > 0)
            StartCoroutine(AI_Loop(null));
    }
}