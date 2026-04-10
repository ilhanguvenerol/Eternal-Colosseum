using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        // Bleed velocity to zero when we arrive in Idle
        Player.CurrentVelocity = Vector3.zero;
        // TODO: Animator trigger "Idle"
        //if(!Player.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Armature|Idle_Loop"))
        Player.Animator.SetState(PlayerAnimator.IDLE);
    }

    public override void Tick()
    {
        // Any movement input → transition out
        if (Player.MoveInput.sqrMagnitude > 0.01f)
        {
            Player.ChangeState(Player.IsRunHeld ? Player.RunState : Player.WalkState);
            return;
        }

        if (Player.DashPressed)
        {
            Player.ChangeState(Player.DashState);
            return;
        }
    }
}