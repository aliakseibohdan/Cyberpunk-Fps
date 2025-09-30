using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLayerMove : MonoBehaviour
{
    public Transform _trackedObject;
    public bool _track;
    public float _rotateSpeed;

    [HideInInspector] public float _deltaX;
    [HideInInspector] public float _deltaY;

    // Use this for initialization
    void Start()
    {
        if (_trackedObject == null)
        {
            _trackedObject = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _deltaX = Mathf.Abs(Mathf.DeltaAngle(this.transform.eulerAngles.x, _trackedObject.eulerAngles.x));
        _deltaY = Mathf.Abs(Mathf.DeltaAngle(this.transform.eulerAngles.y, _trackedObject.eulerAngles.y));

        if (_track)
        {
            if (_rotateSpeed > 0)
            {
                // Snap to position, smooth rotation
                transform.SetPositionAndRotation(_trackedObject.position, Quaternion.Lerp(transform.rotation, _trackedObject.rotation, Mathf.SmoothStep(0.0f, 1.0f, _rotateSpeed)));
            }
            else
            {
                // Snap to both position and rotation
                transform.SetPositionAndRotation(_trackedObject.position, _trackedObject.rotation);
            }
        }
    }
}