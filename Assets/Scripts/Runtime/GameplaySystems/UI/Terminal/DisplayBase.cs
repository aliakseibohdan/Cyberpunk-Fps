using UnityEngine;

public abstract class DisplayBase : MonoBehaviour
{
    [SerializeField] protected float displayLifetime = 2.0f;
    [SerializeField] protected float smoothFollowSpeed = 5.0f;
    [SerializeField] protected float distanceFromCamera = 3.0f;

    protected Transform cameraTransform;
    protected Vector3 targetOffset;
    protected bool isActive = false;

    public virtual void Initialize(Transform cameraTransform, Vector3 worldOffset)
    {
        this.cameraTransform = cameraTransform;
        this.targetOffset = worldOffset;
        isActive = true;

        UpdatePosition();
    }

    protected virtual void Update()
    {
        if (!isActive || cameraTransform == null) return;

        UpdatePosition();
    }

    protected void UpdatePosition()
    {
        Vector3 targetPosition = cameraTransform.position +
                                cameraTransform.forward * distanceFromCamera +
                                targetOffset;

        transform.SetPositionAndRotation(Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothFollowSpeed * Time.deltaTime
        ), Quaternion.LookRotation(
            transform.position - cameraTransform.position
        ));
    }

    public abstract void Show();
    public abstract void Hide();
}