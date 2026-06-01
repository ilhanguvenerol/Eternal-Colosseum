using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Identity")]
    public string SpellName;

    [Tooltip("Match this number to the CombatState value in the Animator graph (e.g., 3 for Consume)")]
    public int AnimatorCombatState = 3;

    [Header("Animation")]
    public AnimationClip CastAnimation;

    [Header("Spell Costs")]
    public float ManaCost = 20f;

    /// <summary>
    /// STRATEGY PATTERN: Overridden by individual spells to execute unique behavior.
    /// </summary>
    public virtual void ExecuteSpell(GameObject caster)
    {
        // Base behavior
    }
}