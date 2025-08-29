using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitchAngle = -80f;
    [SerializeField] private float maxPitchAngle = 80f;

    private float _pitch;
    private float _yaw;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        _yaw = target.eulerAngles.y;
        _pitch = target.eulerAngles.x;

        if (_pitch > 180f) _pitch -= 360f;
    }

    public void UpdateRotation(CameraInput input)
    {
        _yaw += input.Look.x * sensitivity;
        _pitch -= input.Look.y * sensitivity;

        _pitch = Mathf.Clamp(_pitch, minPitchAngle, maxPitchAngle);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}