using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private bool rotateClockwise = true;

    [Header("Vertical Movement Settings")]
    [SerializeField] private bool enableVerticalMovement = false;
    [SerializeField] private float movementAmplitude = 0.5f;
    [SerializeField] private float verticalSpeed = 1f;

    private Vector3 startPosition;
    private float initialY;

    void Start()
    {
        startPosition = transform.position;
        initialY = startPosition.y;
    }

    void Update()
    {
        RotateObject();

        if (enableVerticalMovement)
        {
            MoveVertically();
        }
    }

    private void RotateObject()
    {
        float direction = rotateClockwise ? -1f : 1f;
        transform.Rotate(Vector3.up, rotationSpeed * direction * Time.deltaTime);
    }

    private void MoveVertically()
    {
        float newY = initialY + Mathf.Sin(Time.time * verticalSpeed) * movementAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void ToggleVerticalMovement()
    {
        enableVerticalMovement = !enableVerticalMovement;
    }

    public void SetMovementAmplitude(float newAmplitude)
    {
        movementAmplitude = newAmplitude;
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }

    public void ToggleRotationDirection()
    {
        rotateClockwise = !rotateClockwise;
    }
}