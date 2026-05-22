using UnityEngine;


public class TransformBezierKnot : MonoBehaviour, IBezierKnot, IFlippableObject
{
    public float rotationOffset = 0f;
    public float forwardsHandleSize = 5f;
    public float backwardsHandleSize = 5f;
    public float positionOffset = 0f;

    public Vector3 InverseTransformPoint(Vector3 pos) => transform.InverseTransformPoint(pos);

    public Vector3 RawPosition => transform.position;
    public Quaternion RawRotation => transform.rotation;

    public Vector3 RawForward => transform.forward;
    public Vector3 RawRight => transform.right;
    public Vector3 RawUp => transform.up;

    public Quaternion OffsetQuaternion => Quaternion.AngleAxis(rotationOffset, RawUp);

    public Quaternion Rotation => OffsetQuaternion * RawRotation;
    public Vector3 Position => transform.TransformPoint(Vector3.right * positionOffset);

    public Vector3 Forward => OffsetQuaternion * RawForward;
    public Vector3 Right => OffsetQuaternion * RawForward;
    public Vector3 Up => OffsetQuaternion * RawForward;
    public Vector3 ForwardHandlePosition => ScaledForwardsHandle(1f);
    public Vector3 BackwardsHandlePosition => ScaledBackwardsHandle(1f);

    public Transform TransformReference => this.transform;

    Vector3 ScaledForwardsHandle(float scale)
    {
        return Position + Forward * forwardsHandleSize * scale;
    }

    Vector3 ScaledBackwardsHandle(float scale)
    {
        return Position - Forward * backwardsHandleSize * scale;
    }

    public void Flip()
    {
        Vector3 pos = transform.localPosition;
        Vector3 rot = transform.localEulerAngles;

        pos.x *= -1f;
        rot.y *= -1f;

        transform.localPosition = pos;
        transform.localEulerAngles = rot;

        positionOffset *= -1f;
        rotationOffset *= -1f;
    }
}
