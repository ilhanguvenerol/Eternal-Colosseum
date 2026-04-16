// PlayerController.cs
// Owns the StateMachine and all shared data/components.
// States read from here — they never talk to each other directly.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────

    [Header("Movement Speeds")]
    public float BaseWalkSpeed = 3.5f;
    public float BaseRunSpeed = 7f;
    public float DashSpeed = 18f;
    public float DashDuration = 0.2f;

    // ─────────────────────────────────────────
    // Inventory Speed Integration
    // ─────────────────────────────────────────
    public float WalkSpeed => BaseWalkSpeed + (Inventory.Instance != null ? Inventory.Instance.GetTotalBonusSpeed() : 0);
    public float RunSpeed => BaseRunSpeed + (Inventory.Instance != null ? Inventory.Instance.GetTotalBonusSpeed() : 0);

    [Header("Acceleration / Deceleration")]
    public float Acceleration = 12f;
    public float Deceleration = 16f;

    [Header("Rotation")]
    public float RotationSpeed = 10f;

    [Header("Gravity")]
    public float Gravity = -20f;
    public float GroundedGravity = -2f;

    // ─────────────────────────────────────────
    //  Shared Runtime Data (states read & write)
    // ─────────────────────────────────────────

    public Vector2 MoveInput { get; private set; }
    public bool IsRunHeld { get; private set; }
    public bool DashPressed { get; private set; }

    public Vector3 CurrentVelocity { get; set; }
    public float VerticalVelocity { get; set; }

    // ─────────────────────────────────────────
    //  Components & References
    // ─────────────────────────────────────────

    public CharacterController Controller { get; private set; }
    public Camera MainCamera { get; private set; }
    public PlayerAnimator Animator { get; private set; }

    // ─────────────────────────────────────────
    //  State Machine
    // ─────────────────────────────────────────

    private StateMachine _stateMachine;

    // Expose states so they can reference each other for transitions
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }

    // ─────────────────────────────────────────
    //  Input
    // ─────────────────────────────────────────

    private PlayerInputActions _input;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────

    private void Start()
    {
        Controller = GetComponent<CharacterController>();
        Animator = GetComponent<PlayerAnimator>();
        AttackState = new PlayerAttackState(this);
        MainCamera = Camera.main;

        // Build input
        _input = new PlayerInputActions();
        _input.Enable();
        _input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => MoveInput = Vector2.zero;
        _input.Player.Run.performed += ctx => IsRunHeld = true;
        _input.Player.Run.canceled += ctx => IsRunHeld = false;
        _input.Player.Dash.performed += ctx => DashPressed = true;  // state will consume this
        _input.Player.Attack.performed += ctx => {
            if (_stateMachine.CurrentState != DashState) // Don't attack while dashing
                ChangeState(AttackState);
        };

        // Build states (pass 'this' so every state has access to shared data)
        _stateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        RunState = new PlayerRunState(this);
        DashState = new PlayerDashState(this);

        // Start in Idle (safely, after all Awakes have finished)
        _stateMachine.ChangeState(IdleState);
    }

    private void OnEnable() => _input?.Enable();
    private void OnDisable() => _input?.Disable();

    private void Update()
    {
        _stateMachine.Tick();

        // Apply final movement
        Controller.Move((CurrentVelocity + Vector3.up * VerticalVelocity) * Time.deltaTime);

        // Gravity
        if (Controller.isGrounded)
            VerticalVelocity = GroundedGravity;
        else
            VerticalVelocity += Gravity * Time.deltaTime;

        // Reset one-shot inputs after the state machine has had a chance to read them
        DashPressed = false;
    }

    private void FixedUpdate() => _stateMachine.FixedTick();

    // ─────────────────────────────────────────
    //  Helpers (shared logic all states use)
    // ─────────────────────────────────────────

    /// <summary>
    /// Returns a camera-relative world direction from raw WASD/stick input.
    /// </summary>
    public Vector3 GetCameraRelativeInputDirection()
    {
        if (MoveInput.sqrMagnitude < 0.01f) return Vector3.zero;

        Vector3 camForward = Vector3.ProjectOnPlane(MainCamera.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(MainCamera.transform.right, Vector3.up).normalized;
        return (camForward * MoveInput.y + camRight * MoveInput.x).normalized;
    }

    /// <summary>
    /// Rotates the character toward a world-space direction.
    /// </summary>
    public void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, RotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Tells the state machine to switch to a new state.
    /// </summary>
    public void ChangeState(PlayerBaseState newState) => _stateMachine.ChangeState(newState);

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.Label(new UnityEngine.Rect(10, 10, 300, 20),
            $"State: {_stateMachine.CurrentState?.GetType().Name}");
    }
#endif
}
