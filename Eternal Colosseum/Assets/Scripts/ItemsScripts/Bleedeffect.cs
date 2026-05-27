using System.Collections;
using UnityEngine;

/// <summary>
/// Attached at runtime to an enemy by SerratedArenaBladeItem when bleed procs.
/// Deals damage over time and then removes itself.
/// Safe to call StartBleed() multiple times — refreshes the duration.
/// </summary>
public class BleedEffect : MonoBehaviour
{
    private Coroutine _activeBleed;

    /// <summary>Start or refresh a bleed on this enemy.</summary>
    public void StartBleed(float damagePerTick, float duration, float tickRate = 0.5f)
    {
        if (_activeBleed != null)
            StopCoroutine(_activeBleed); // Refresh — restart the bleed timer

        _activeBleed = StartCoroutine(BleedRoutine(damagePerTick, duration, tickRate));
    }

    private IEnumerator BleedRoutine(float dmg, float duration, float rate)
    {
        float elapsed = 0f;
        EnemyHealth hp = GetComponentInParent<EnemyHealth>();

        while (elapsed < duration)
        {
            if (hp == null || hp.IsDead) break;

            hp.TakeDamage(dmg, null);
            yield return new WaitForSeconds(rate);
            elapsed += rate;
        }

        _activeBleed = null;
        Destroy(this); // Clean up after ourselves
    }
}