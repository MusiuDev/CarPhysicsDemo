using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroupBezierSpline : MonoBehaviour
{
    public TransformBezierKnot[] Knots;
}

[System.Serializable]
public class TransformBezierKnot : IBezierKnot
{
    [SerializeField] private Transform _transform;
    public float rotationOffset;
    public float forwardsHandleSize;
    public float backwardsHandleSize;
    public float positionOffset;

    public Vector3 InverseTransformPoint(Vector3 pos) => _transform.InverseTransformPoint(pos);

    public Vector3 RawPosition => _transform.position;
    public Quaternion RawRotation => _transform.rotation;

    public Vector3 RawForward => _transform.forward;
    public Vector3 RawRight => _transform.right;
    public Vector3 RawUp => _transform.up;

    public Quaternion OffsetQuaternion => Quaternion.AngleAxis(rotationOffset, _transform.up);
    
    public Quaternion Rotation => OffsetQuaternion * _transform.rotation;
    public Vector3 Position => _transform.TransformPoint(Vector3.right * positionOffset);

    public Vector3 Forward => OffsetQuaternion * _transform.forward;
    public Vector3 Right => OffsetQuaternion * _transform.right;
    public Vector3 Up => OffsetQuaternion * _transform.up;
    public Vector3 ForwardHandlePosition => ScaledForwardsHandle(1f);
    public Vector3 BackwardsHandlePosition => ScaledBackwardsHandle(1f);

    Vector3 ScaledForwardsHandle(float scale)
    {
        return Position + Forward * forwardsHandleSize * scale;
    }

    Vector3 ScaledBackwardsHandle(float scale)
    {
        return Position - Forward * backwardsHandleSize * scale;
    }


    public TransformBezierKnot(Transform transform, float rotationOffset, float forwardsHandleSize, float backwardsHandleSize)
    {
        this._transform = transform;
        this.rotationOffset = rotationOffset;
        this.forwardsHandleSize = forwardsHandleSize;
        this.backwardsHandleSize = backwardsHandleSize;
    }
}
