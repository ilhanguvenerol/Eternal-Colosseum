// PlayerAnimator.cs
// Single place that talks to the Animator.
// States call methods here — they never touch the Animator directly.
// This keeps animator parameter names out of every state file.

using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // ── Animator parameter IDs ────────────────────────────────────────────────
    // Hashed once at startup — faster than string lookups every frame.
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    // ── State integer values ──────────────────────────────────────────────────
    // Matches the integer values you set on each transition in the Animator.
    public const int IDLE = 0;
    public const int WALK = 1;
    public const int RUN = 2;
    public const int DASH = 3;
    // To add more: public const int CROUCH = 4; — then add a transition in Animator.

    private Animator _animator;

    private void Start() => _animator = GetComponent<Animator>();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the active locomotion state. Call once in each state's Enter().
    /// </summary>
    public void SetState(int state)
    {
        _animator.SetInteger(StateHash, state);
    }

    /// <summary>
    /// Sets the normalised movement speed (0–1) for blend trees.
    /// Call every Tick() in movement states.
    /// </summary>
    public void SetSpeed(float speed)
    {
        _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime); // 0.1s damp prevents jitter
    }

    /// <summary>
    /// Force-exits any current animation immediately and goes to a state by name.
    /// Only use this for hard interrupts (e.g. hit reaction, death).
    /// </summary>
    public void PlayImmediate(string stateName, int layer = 0)
    {
        _animator.Play(stateName, layer, 0f);
    }
}