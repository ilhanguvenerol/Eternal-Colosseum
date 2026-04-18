using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _attackDuration = 0.6f; // Length of your swing animation
    private float _timer;

    public PlayerAttackState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        _timer = _attackDuration;

        // 1. Decoupled Animation Trigger
        // We no longer force an integer state. This lets the Animator handle 
        Player.Animator.TriggerAttack();

        // 2. Movement Logic
        // Keeping this zeroed ensures the attack has "weight" 
        Player.CurrentVelocity = Vector3.zero;

        // 3. Inventory Handshake
        // Pulling the real-time damage from your ScriptableObjects
        if (Inventory.Instance != null && Inventory.Instance.equippedWeapon != null)
        {
            float totalDamage = Inventory.Instance.equippedWeapon.baseDamage + Inventory.Instance.GetTotalBonusDamage();

            Debug.Log($"[COMBAT] Attacking with: {Inventory.Instance.equippedWeapon.weaponName}");
            Debug.Log($"[STATS] Total Calculated Damage: {totalDamage}");
        }
        else
        {
            Debug.LogWarning("[COMBAT] Attack triggered but no weapon is equipped!");
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