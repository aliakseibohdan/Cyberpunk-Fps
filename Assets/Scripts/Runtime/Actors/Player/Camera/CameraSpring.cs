using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)]
    [SerializeField] private float halfLife = 0.075f;
    [Space]
    [SerializeField] private float frequency = 18f;
    [Space]
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    [Space]
    [Header("Movement Slide")]
    [SerializeField] private float forwardSlideAmount = 0.1f;
    [SerializeField] private float slideResponse = 8f;

    private Vector3 _springPosition;
    private Vector3 _springVelocity;
    private float _currentSlideOffset;
    private float _targetSlideOffset;

    public void Initialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
        _currentSlideOffset = 0f;
        _targetSlideOffset = 0f;
    }

    public void UpdateSpring(float deltaTime, Vector3 up, float forwardInput)
    {
        transform.localPosition = Vector3.zero;

        // Update slide target based on forward input
        _targetSlideOffset = forwardInput * forwardSlideAmount;
        
        // Smoothly interpolate toward target slide offset
        _currentSlideOffset = Mathf.Lerp(
            _currentSlideOffset,
            _targetSlideOffset,
            1 - Mathf.Exp(-slideResponse * deltaTime)
        );

        Spring(ref _springPosition, ref _springVelocity, transform.position, halfLife, frequency, deltaTime);

        var relativeSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(relativeSpringPosition, up);

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
        
        // Apply both the original spring effect and the slide offset
        transform.position += relativeSpringPosition * linearDisplacement;
        transform.position += transform.forward * _currentSlideOffset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, _springPosition);
        Gizmos.DrawSphere(_springPosition, 0.1f);
        
        // Draw slide direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * _currentSlideOffset);
    }

    public void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}