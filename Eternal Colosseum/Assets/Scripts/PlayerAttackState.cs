using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _attackDuration = 0.6f; // Length of your swing animation
    private float _timer;

    public PlayerAttackState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        _timer = _attackDuration;

        // Ensure the player doesn't slide while attacking
        Player.CurrentVelocity = Vector3.zero; //Attack animations are handled separately from movement, player should walk and attack simultaneously

        // 1. Trigger the animation using the constant from your Animator script
        // Note: Make sure you added 'public const int ATTACK = 4;' to PlayerAnimator.cs
        //Player.Animator.SetState(PlayerAnimator.ATTACK);

        // 2. Inventory Logic: Calculate damage using your modular system
        if (Inventory.Instance.equippedWeapon != null)
        {
            float totalDamage = Inventory.Instance.equippedWeapon.baseDamage + Inventory.Instance.GetTotalBonusDamage();

            Debug.Log($"[INVENTORY CHECK] Weapon: {Inventory.Instance.equippedWeapon.weaponName}");
            Debug.Log($"[COMBAT LOG] Total Damage: {totalDamage} (Base + Bonuses)");
        }
        else
        {
            Debug.LogWarning("No weapon equipped in Inventory!");
        }
    }

    public override void Tick()
    {
        _timer -= Time.deltaTime;

        // Transition back to Idle once the timer expires
        if (_timer <= 0f)
        {
            Player.ChangeState(Player.IdleState);
        }
    }
}