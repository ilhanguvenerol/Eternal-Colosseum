// StateMachine.cs
// Generic runner — knows nothing about players, enemies, etc.
// Any MonoBehaviour can own one of these.

public class StateMachine
{
    private IState _currentState;

    public IState CurrentState => _currentState;

    public void ChangeState(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Tick() => _currentState?.Tick();
    public void FixedTick() => _currentState?.FixedTick();
}