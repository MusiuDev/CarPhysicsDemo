using System.Collections.Generic;
using UnityEngine;

public class BezierSpline : MonoBehaviour
{
    [SerializeField] private BezierKnot[] _knots;
    public IReadOnlyCollection<BezierKnot> Knots => _knots;

    public List<Vector3> GetFullPath(float segmentLength)
    {
        if (_knots == null || _knots.Length == 0) return new List<Vector3>() { Vector3.zero };
        if (_knots.Length == 1) return new List<Vector3>() { _knots[0].position };

        List<Vector3> path = new List<Vector3>();
        float nextOffset = 0f;

        for (int i = 0; i < _knots.Length - 1; i++)
        {
            Bezier curve = new Bezier(_knots[i], _knots[i + 1]);
            Vector3[] points = curve.GetEquallySpacedPoints(segmentLength, out float remainingDistance, nextOffset);
            nextOffset = segmentLength - remainingDistance;
            path.AddRange(points);
        }
        return path;
    }


}

[System.Serializable]
public class TransformBezierKnot_old
{
    public Transform transform;
    public float forwardsHandleSize;
    public float backwardsHandleSize;
    public float handleOffset;

    public Vector3 RawPosition => transform ? transform.position : Vector3.zero;
    public Vector3 RawForwards => transform ? transform.forward : Vector3.forward;
    public Vector3 RawRight => transform ? transform.right : Vector3.right;

    public Vector3 Position => RawPosition + RawRight * handleOffset;
    public Vector3 ForwardsHandle => GetScaledBackwardsHandle(1f);
    public Vector3 BackwardsHandle => GetScaledBackwardsHandle(1f);

    public Vector3 GetScaledForwardsHandle(float scale)
    {
        return Position + RawForwards * forwardsHandleSize * scale;
    }

    public Vector3 GetScaledBackwardsHandle(float scale)
    {
        return Position - RawForwards * backwardsHandleSize * scale;
    }

    public TransformBezierKnot_old(Transform transform, float forwardsHandleSize, float backwardsHandleSize, float handleOffset)
    {
        this.transform = transform;
        this.forwardsHandleSize = forwardsHandleSize;
        this.backwardsHandleSize = backwardsHandleSize;
        this.handleOffset = handleOffset;
    }

    public static BezierDefinition GetDefinition(TransformBezierKnot_old from, TransformBezierKnot_old to)
    {
        Vector3 p0 = from.Position;
        Vector3 p1 = from.ForwardsHandle;

        Vector3 p2 = to.BackwardsHandle;
        Vector3 p3 = to.Position;

        return new BezierDefinition(p0, p1, p2, p3);
    }
}

[System.Serializable]
public class BezierKnot
{
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
    public float forwardsHandleSize;
    public float backwardsHandleSize;

    public Vector3 Forward => rotation * Vector3.forward;
    public Vector3 Right => rotation * Vector3.right;
    public Vector3 Up => rotation * Vector3.up;

    public Vector3 ForwardHandlePosition => ScaledForwardsHandle(1f);
    public Vector3 BackwardsHandlePosition => ScaledBackwardsHandle(1f);

    public Vector3 ScaledForwardsHandle(float scale)
    {
        return position + Forward * forwardsHandleSize * scale;
    }

    public Vector3 ScaledBackwardsHandle(float scale)
    {
        return position - Forward * backwardsHandleSize * scale;
    }

    public BezierKnot(Vector3 position, Quaternion rotation, float forwardsHandleSize, float backwardsHandleSize)
    {
        this.position = position;
        this.rotation = rotation;
        this.forwardsHandleSize = forwardsHandleSize;
        this.backwardsHandleSize = backwardsHandleSize;
    }
}