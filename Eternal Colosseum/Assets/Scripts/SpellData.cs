// SpellData.cs
// Create one ScriptableObject asset per spell.
// Right-click in Project → Create → Combat → Spell Data

using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Identity")]
    public string SpellName;

    [Header("Animation")]
    public AnimationClip CastAnimation;   // the unique clip for this spell

    // ── Extend here later ─────────────────────────────────────────────────────
    // public float        CastTime;
    // public float        ManaCost;
    // public GameObject   ProjectilePrefab;
    // public AudioClip    CastSound;
    // public ParticleSystem CastVFX;
}