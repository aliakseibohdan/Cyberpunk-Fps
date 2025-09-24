using UnityEngine;

public class MouseController : MonoBehaviour
{
    [Header("Mouse Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float easingFactor = 0.1f;
    [SerializeField] private float maxRadius = 3f;

    [Header("Reference Points")]
    [SerializeField] private Transform centerPoint;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private Vector2 mouseStartPosition;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        HandleMouseInput();
        ApplySmoothMovement();
        ApplyRadiusConstraint();
    }

    private void Initialize()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition;

        mouseStartPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        isInitialized = true;
    }

    private void HandleMouseInput()
    {
        if (!isInitialized) return;

        Vector2 currentMousePos = Input.mousePosition;

        Vector2 mouseDelta = currentMousePos - mouseStartPosition;

        Vector2 normalizedDelta = new(
            mouseDelta.x / Screen.width,
            mouseDelta.y / Screen.height
        );

        Vector3 center = centerPoint != null ? centerPoint.position : initialPosition;
        targetPosition = center + new Vector3(normalizedDelta.y, 0f, -normalizedDelta.x) * movementSpeed;
    }

    private void ApplySmoothMovement()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, easingFactor);
    }

    private void ApplyRadiusConstraint()
    {
        Vector3 center = centerPoint != null ? centerPoint.position : initialPosition;
        Vector3 currentOffset = transform.position - center;

        if (currentOffset.magnitude > maxRadius)
        {
            Vector3 clampedPosition = center + currentOffset.normalized * maxRadius;
            transform.position = clampedPosition;
            targetPosition = clampedPosition;
        }
    }

    public void SetMaxRadius(float newRadius)
    {
        maxRadius = Mathf.Max(0f, newRadius);
    }

    public void SetMovementSpeed(float newSpeed)
    {
        movementSpeed = Mathf.Max(0f, newSpeed);
    }

    public void SetEasingFactor(float newEasing)
    {
        easingFactor = Mathf.Clamp(newEasing, 0.01f, 1f);
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
        targetPosition = initialPosition;

        mouseStartPosition = Input.mousePosition;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = centerPoint != null ? centerPoint.position :
                         Application.isPlaying ? initialPosition : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, maxRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, transform.position);
        }
    }
}