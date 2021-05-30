using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Camera-Control/CameraController")]
public class CameraController : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXandY;
    public float sensitivityX = 2.0f;
    public float sensitivityY = 2.0f;
    public float minX = -360.0f;
    public float maxX = 360.0f;
    public float minY = -360.0f;
    public float maxY = 360.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private Quaternion originalRotation;

    void Start()
    {
        if(GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if(axes == RotationAxes.MouseXandY)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

            rotationX = ClampAngle(rotationX, minX, maxX);
            rotationY = ClampAngle(rotationY, minY, maxY);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if(axes == RotationAxes.MouseX)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationX = ClampAngle(rotationX, minX, maxX);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else if (axes == RotationAxes.MouseY)
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = ClampAngle(rotationY, minY, maxY);
            Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
            transform.localRotation = originalRotation * yQuaternion;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360.0f) angle += 360.0f;
        if (angle > 360.0f) angle -= 360.0f;

        return Mathf.Clamp(angle, min, max);
    } 
}
