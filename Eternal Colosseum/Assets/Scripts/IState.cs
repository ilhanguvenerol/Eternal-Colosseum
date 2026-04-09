// IState.cs
// Every state implements this contract.

public interface IState
{
    void Enter();       // called once on state entry
    void Tick();        // called every Update()
    void FixedTick();   // called every FixedUpdate()
    void Exit();        // called once state exit
}
