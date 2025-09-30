using UnityEngine;

public class MimicRotation : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target object whose rotation will be mimicked")]
    public Transform targetObject;

    [Header("Mimic Settings")]
    [Tooltip("Whether to mimic rotation smoothly")]
    public bool smoothRotation = false;

    [Tooltip("Rotation speed when using smooth rotation")]
    [Range(0.1f, 10f)]
    public float rotationSpeed = 2f;

    [Tooltip("Mimic rotation on all axes")]
    public bool mimicAllAxes = true;

    [Tooltip("Select which axes to mimic when not using all axes")]
    public Vector3 enabledAxes = Vector3.one;

    private void Update()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Target object not assigned in MimicRotation script!");
            return;
        }

        Quaternion targetRotation = GetTargetRotation();

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    private Quaternion GetTargetRotation()
    {
        if (mimicAllAxes)
            return targetObject.rotation;

        Vector3 eulerAngles = transform.rotation.eulerAngles;
        Vector3 targetEuler = targetObject.rotation.eulerAngles;

        return Quaternion.Euler(
            Mathf.LerpAngle(eulerAngles.x, targetEuler.x, enabledAxes.x),
            Mathf.LerpAngle(eulerAngles.y, targetEuler.y, enabledAxes.y),
            Mathf.LerpAngle(eulerAngles.z, targetEuler.z, enabledAxes.z)
        );
    }

    public void SetTarget(Transform newTarget)
    {
        targetObject = newTarget;
    }
}