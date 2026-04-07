// PlayerBaseState.cs
// All player states inherit from this.
// Gives every state access to the PlayerController without repeating the reference.

public abstract class PlayerBaseState : IState
{
    protected PlayerController Player;  // the MonoBehaviour that owns the state machine

    protected PlayerBaseState(PlayerController player)
    {
        Player = player;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
    public virtual void Exit() { }
}