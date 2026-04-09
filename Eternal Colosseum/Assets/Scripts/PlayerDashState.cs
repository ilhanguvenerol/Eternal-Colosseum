// PlayerDashState.cs

using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _dashTimer;
    private Vector3 _dashDirection;

    public PlayerDashState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        _dashTimer = Player.DashDuration;

        // Dash in movement direction, fall back to facing direction
        _dashDirection = Player.CurrentVelocity.sqrMagnitude > 0.01f
            ? Player.CurrentVelocity.normalized
            : Player.transform.forward;

        // TODO: Animator trigger "Dash"
        // TODO: Spawn dash VFX / trail here
    }

    public override void Tick()
    {
        _dashTimer -= Time.deltaTime;

        // Drive velocity at full dash speed — no acceleration needed
        Player.CurrentVelocity = _dashDirection * Player.DashSpeed;

        // Dash finished → return to idle or walk based on input. Just return to idle and let the idle state handle the walk transition, otherwise triggers may reverse and walk animation may stuck in a loop.
        if (_dashTimer <= 0f)
        {
            Player.ChangeState(
                Player.IdleState
            );
        }
    }

    public override void Exit()
    {
        // Bleed some speed out so the exit doesn't feel abrupt
        Player.CurrentVelocity = _dashDirection * Player.RunSpeed;
    }
}