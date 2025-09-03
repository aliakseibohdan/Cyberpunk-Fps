using UnityEngine;

public class UICameraFollower : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float defaultDistance = 3.0f;
    [SerializeField] private float followSmoothness = 5.0f;

    private void Update()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                return;
        }

        Vector3 targetPosition = cameraTransform.position +
                                cameraTransform.forward * defaultDistance;

        transform.SetPositionAndRotation(Vector3.Lerp(
            transform.position,
            targetPosition,
            followSmoothness * Time.deltaTime
        ), Quaternion.LookRotation(
            transform.position - cameraTransform.position
        ));
    }
}