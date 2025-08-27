using UnityEngine;

public class RobotAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private Animator animator;

    private int _speedParam;
    private int _groundedParam;
    private int _stanceParam;
    private int _jumpParam;
    private int _shootParam;
    private int _verticalVelocityParam;

    private void Start()
    {
        _speedParam = Animator.StringToHash("Speed");
        _groundedParam = Animator.StringToHash("Grounded");
        _stanceParam = Animator.StringToHash("Stance");
        _jumpParam = Animator.StringToHash("Jump");
        _shootParam = Animator.StringToHash("Shoot");
        _verticalVelocityParam = Animator.StringToHash("VerticalVelocity");

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var state = playerCharacter.GetState();
        var velocity = state.Velocity;

        // Calculate speed relative to character forward direction
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        float forwardSpeed = Mathf.Clamp(localVelocity.z, -1f, 1f);

        // Update animator parameters
        animator.SetFloat(_speedParam, Mathf.Abs(forwardSpeed));
        animator.SetBool(_groundedParam, state.Grounded);
        animator.SetInteger(_stanceParam, (int)state.Stance);
        animator.SetFloat(_verticalVelocityParam, velocity.y);

        // Trigger jump animation when leaving ground with upward velocity
        if (!state.Grounded && velocity.y > 0)
        {
            animator.SetTrigger(_jumpParam);
        }
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