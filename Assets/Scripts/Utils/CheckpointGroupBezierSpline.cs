using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroupBezierSpline : MonoBehaviour
{
    public TransformBezierKnot[] knots;
    public float segmentDistance = 0.5f;

    void OnDrawGizmos()
    {
        if (knots == null) return;
        if (knots.Length <= 1) return;

        var path = BezierSpline.GetFullPath(knots, segmentDistance);

        if (path == null) return;
        if (path.Count <= 1) return;

        Vector3[] lineList = new Vector3[(path.Count - 1) * 2];
        for (int i = 0; i < path.Count - 1; i++)
        {
            lineList[2 * i] = path[i];
            lineList[2 * i + 1] = path[i + 1];
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLineList(lineList);
    }
}

[System.Serializable]
public class TransformBezierKnot : IBezierKnot
{
    [SerializeField] private Transform _transform;
    public float rotationOffset;
    public float forwardsHandleSize;
    public float backwardsHandleSize;
    public float positionOffset;

    public Vector3 InverseTransformPoint(Vector3 pos) => _transform ? _transform.InverseTransformPoint(pos) : pos;

    public Vector3 RawPosition => _transform ? _transform.position : Vector3.zero;
    public Quaternion RawRotation => _transform ? _transform.rotation : Quaternion.identity;

    public Vector3 RawForward => _transform ? _transform.forward : Vector3.forward;
    public Vector3 RawRight => _transform ? _transform.right : Vector3.right;
    public Vector3 RawUp => _transform ? _transform.up : Vector3.up;

    public Quaternion OffsetQuaternion => Quaternion.AngleAxis(rotationOffset, RawUp);

    public Quaternion Rotation => OffsetQuaternion * RawRotation;
    public Vector3 Position => _transform ? _transform.TransformPoint(Vector3.right * positionOffset) : Vector3.right * positionOffset;

    public Vector3 Forward => OffsetQuaternion * RawForward;
    public Vector3 Right => OffsetQuaternion * RawForward;
    public Vector3 Up => OffsetQuaternion * RawForward;
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
