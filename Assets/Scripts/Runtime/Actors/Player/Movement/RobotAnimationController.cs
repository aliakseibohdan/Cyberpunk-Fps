using UnityEngine;

public class RobotAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private float turnThreshold = 45f;
    [SerializeField] private float quickTurnThreshold = 120f;
    [SerializeField] private float turnCooldown = 0.5f;
    [SerializeField] private float turnCompletionTime = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Parameter IDs
    private int _velocityXParam;
    private int _velocityZParam;
    private int _groundedParam;
    private int _stanceParam;
    private int _jumpParam;
    private int _shootParam;
    private int _turnParam;

    // Turning state
    private float _previousCameraYaw;
    private float _turnVelocity;
    private float _timeSinceLastTurn;
    private float _timeSinceTurnStarted;
    private bool _isTurning;
    private float _currentTurnValue;

    private void Start()
    {
        _velocityXParam = Animator.StringToHash("VelocityX");
        _velocityZParam = Animator.StringToHash("VelocityZ");
        _groundedParam = Animator.StringToHash("Grounded");
        _stanceParam = Animator.StringToHash("Stance");
        _jumpParam = Animator.StringToHash("Jump");
        _shootParam = Animator.StringToHash("Shoot");
        _turnParam = Animator.StringToHash("Turn");

        if (animator == null)
            animator = GetComponent<Animator>();

        _previousCameraYaw = playerCamera.transform.eulerAngles.y;
        _timeSinceLastTurn = turnCooldown;

        if (showDebugInfo)
            Debug.Log("RobotAnimationController initialized with PlayerCamera reference.");
    }

    private void Update()
    {
        var state = playerCharacter.GetState();
        var velocity = state.Velocity;
        float deltaTime = Time.deltaTime;

        float currentCameraYaw = playerCamera.transform.eulerAngles.y;
        float yawDifference = Mathf.DeltaAngle(_previousCameraYaw, currentCameraYaw);
        _turnVelocity = yawDifference / deltaTime;
        _previousCameraYaw = currentCameraYaw;

        _timeSinceLastTurn += deltaTime;

        if (_isTurning)
        {
            _timeSinceTurnStarted += deltaTime;

            if (_timeSinceTurnStarted >= turnCompletionTime)
            {
                _isTurning = false;
                _currentTurnValue = 0f;
                animator.SetFloat(_turnParam, 0f);
            }
        }

        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        float maxSpeed = GetMaxSpeedForStance(state.Stance);
        float normalizedVelocityX = Mathf.Clamp(localVelocity.x / maxSpeed, -1f, 1f);
        float normalizedVelocityZ = Mathf.Clamp(localVelocity.z / maxSpeed, -1f, 1f);

        bool canTurn = state.Grounded && velocity.magnitude < 0.1f && _timeSinceLastTurn >= turnCooldown && !_isTurning;

        if (showDebugInfo)
        {
            Debug.Log($"Turn Velocity: {_turnVelocity}, Can Turn: {canTurn}, Is Turning: {_isTurning}, Time Since Last Turn: {_timeSinceLastTurn}");
        }

        if (canTurn)
        {
            CheckForTurnAnimation();
        }

        animator.SetFloat(_velocityXParam, normalizedVelocityX);
        animator.SetFloat(_velocityZParam, normalizedVelocityZ);
        animator.SetBool(_groundedParam, state.Grounded);
        animator.SetInteger(_stanceParam, (int)state.Stance);

        if (showDebugInfo)
        {
            Debug.Log($"Animator Turn Value: {animator.GetFloat(_turnParam)}");
        }

        if (playerCharacter.RequestedJump)
        {
            animator.SetTrigger(_jumpParam);
        }
    }

    private void CheckForTurnAnimation()
    {
        if (Mathf.Abs(_turnVelocity) > quickTurnThreshold)
        {
            // 180-degree turn (quick turn)
            _currentTurnValue = Mathf.Sign(_turnVelocity) * 2f;
            animator.SetFloat(_turnParam, _currentTurnValue);
            _timeSinceLastTurn = 0f;
            _timeSinceTurnStarted = 0f;
            _isTurning = true;

            if (showDebugInfo)
                Debug.Log($"Quick turn triggered: {_currentTurnValue}");
        }
        else if (Mathf.Abs(_turnVelocity) > turnThreshold)
        {
            // 90-degree turn
            _currentTurnValue = Mathf.Sign(_turnVelocity) * 1f;
            animator.SetFloat(_turnParam, _currentTurnValue);
            _timeSinceLastTurn = 0f;
            _timeSinceTurnStarted = 0f;
            _isTurning = true;

            if (showDebugInfo)
                Debug.Log($"Normal turn triggered: {_currentTurnValue}");
        }
    }

    private float GetMaxSpeedForStance(Stance stance)
    {
        return stance switch
        {
            Stance.Stand => 20f,
            Stance.Crouch => 7f,
            Stance.Slide => 25f,
            _ => 20f,
        };
    }

    public void OnTurnComplete()
    {
        _isTurning = false;
        _currentTurnValue = 0f;
        animator.SetFloat(_turnParam, 0f);
    }

    public void OnShootStart()
    {
        animator.SetBool(_shootParam, true);
    }

    public void OnShootEnd()
    {
        animator.SetBool(_shootParam, false);
    }

    public void OnFootstep(AnimationEvent evt)
    {
        // Play footstep sound based on surface material
        // Add surface detection logic here
    }

    public void OnLand(AnimationEvent evt)
    {
        // Play landing sound or effect
    }

    public void OnSlideStart(AnimationEvent evt)
    {
        // Play slide sound or effect
    }
}