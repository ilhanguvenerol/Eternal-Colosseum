// PlayerRunState.cs

using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        // TODO: Animator trigger "Run"
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

        if (!Player.IsRunHeld)
        {
            Player.ChangeState(Player.WalkState);
            return;
        }

        // ── Movement ─────────────────────────────
        Move(Player.RunSpeed);
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