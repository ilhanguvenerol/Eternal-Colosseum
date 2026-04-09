// PlayerWalkState.cs

using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        // TODO: Animator trigger "Walk"
    }

    public override void Tick()
    {
        // ── Transitions ──────────────────────────
        if (Player.DashPressed)
        {
            Player.ChangeState(Player.DashState);
            return;
        }

        if (Player.MoveInput.sqrMagnitude < 0.01f)
        {
            Player.ChangeState(Player.IdleState);
            return;
        }

        if (Player.IsRunHeld)
        {
            Player.ChangeState(Player.RunState);
            return;
        }

        // ── Movement ─────────────────────────────
        Move(Player.WalkSpeed);
    }

    private void Move(float targetSpeed)
    {
        Vector3 direction = Player.GetCameraRelativeInputDirection();
        float currentSpeed = Player.CurrentVelocity.magnitude;
        float smoothed = Mathf.MoveTowards(currentSpeed, targetSpeed, Player.Acceleration * Time.deltaTime);

        Player.CurrentVelocity = direction * smoothed;
        Player.RotateToward(direction);
    }
}