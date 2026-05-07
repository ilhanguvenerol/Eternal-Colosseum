using UnityEngine;

/// <summary>
/// Abstract base for all enemy movement states.
/// Each state owns its own entry, per-frame update, and exit logic.
/// Attack behaviour is intentionally excluded — this layer is movement only.
/// </summary>
public abstract class EnemyState
{
    protected EnemyBrain brain;
    protected Transform player;

    public EnemyState(EnemyBrain brain)
    {
        this.brain  = brain;
        this.player = brain.Player;
    }

    /// <summary>Called once when the state is entered.</summary>
    public virtual void Enter() { }

    /// <summary>Called every frame while this state is active.</summary>
    public virtual void Update() { }

    /// <summary>Called once just before the state is replaced.</summary>
    public virtual void Exit() { }
}
