using UnityEngine;

public class RigidbodyRotator : MonoBehaviour
{
    public float rotationSpeed;
    public RotationAxis rotationAxis;
    public new Rigidbody rigidbody;

    private float _currentRotation;

    void FixedUpdate()
    {
        _currentRotation = ((_currentRotation + Time.fixedDeltaTime * rotationSpeed) + 360f) % 360f;
        Vector3 rotation;
        switch (rotationAxis)
        {
            case RotationAxis.YAxis:
                rotation = Vector3.up;
                break;
            case RotationAxis.XAxis:
                rotation = Vector3.right;
                break;
            case RotationAxis.ZAxis:
                rotation = Vector3.forward;
                break;
            default:
                rotation = Vector3.up;
                break;
        }

        rigidbody.MoveRotation(Quaternion.Euler(rotation * _currentRotation));
    }
}

public enum RotationAxis
{
    YAxis,
    XAxis,
    ZAxis
}
