using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.2f;    // seconds the dash lasts

    [Header("Acceleration / Deceleration")]
    [SerializeField] private float acceleration = 12f;   // how fast we reach target speed
    [SerializeField] private float deceleration = 16f;   // how fast we stop

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;   // how fast the character turns

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;  // small constant to keep grounded

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;    // assign your Camera or CameraRoot

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────

    private CharacterController _controller;
    private PlayerInputActions _input;          // generated C# class from Input Actions asset

    // Input values
    private Vector2 _moveInput;
    private bool _isRunHeld;

    // Velocity
    private Vector3 _currentVelocity;            // XZ smoothed velocity
    private float _verticalVelocity;

    // Dash
    private bool _isDashing;
    private float _dashTimer;
    private Vector3 _dashDirection;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        _input = new PlayerInputActions();

        // Move (Vector2 read every frame in Update)
        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        // Run (hold)
        _input.Player.Run.performed += ctx => _isRunHeld = true;
        _input.Player.Run.canceled += ctx => _isRunHeld = false;

        // Dash (tap)
        _input.Player.Dash.performed += ctx => TryDash();
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
        HandleDashTimer();
        HandleMovement();
        HandleGravity();

        _controller.Move((_currentVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);
    }

    // ─────────────────────────────────────────
    //  Movement
    // ─────────────────────────────────────────

    private void HandleMovement()
    {
        if (_isDashing)
        {
            // During dash, override XZ with fixed dash velocity
            _currentVelocity = _dashDirection * dashSpeed;
            return;
        }

        // Build a world-space direction relative to the camera
        Vector3 inputDir = Vector3.zero;
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            // Camera-relative movement: forward/right projected onto XZ plane
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            inputDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
        }

        // Choose target speed
        float targetSpeed = inputDir.sqrMagnitude > 0.01f
            ? (_isRunHeld ? runSpeed : walkSpeed)
            : 0f;

        // Smooth the velocity magnitude (acceleration / deceleration)
        float currentSpeed = _currentVelocity.magnitude;
        float speedDelta = targetSpeed > currentSpeed ? acceleration : deceleration;
        float smoothedSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedDelta * Time.deltaTime);

        // Apply to world direction
        _currentVelocity = inputDir * smoothedSpeed;

        // Rotate character to face movement direction
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────
    //  Gravity
    // ─────────────────────────────────────────

    private void HandleGravity()
    {
        if (_controller.isGrounded)
        {
            _verticalVelocity = groundedGravity;   // keeps controller grounded on slopes
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    // ─────────────────────────────────────────
    //  Dash
    // ─────────────────────────────────────────

    private void TryDash()
    {
        if (_isDashing) return;   // already dashing — ignore (add cooldown here if desired)

        // Dash in current facing direction; fall back to transform.forward if standing still
        _dashDirection = _currentVelocity.sqrMagnitude > 0.01f
            ? _currentVelocity.normalized
            : transform.forward;

        _isDashing = true;
        _dashTimer = dashDuration;
    }

    private void HandleDashTimer()
    {
        if (!_isDashing) return;

        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0f)
            _isDashing = false;
    }

    // ─────────────────────────────────────────
    //  Gizmos (editor helper)
    // ─────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, _currentVelocity);
    }
#endif
}