using System.Collections.Generic;
using UnityEngine;

public class BezierSpline : MonoBehaviour
{
    [SerializeField] private TransformBezierKnot[] _knots;
    public IReadOnlyCollection<TransformBezierKnot> Knots => _knots;

    public List<Vector3> GetFullPath(float segmentLength)
    {
        if (_knots == null || _knots.Length == 0) return new List<Vector3>() { Vector3.zero };
        if (_knots.Length == 1) return new List<Vector3>() { _knots[0].Position };

        List<Vector3> path = new List<Vector3>();
        float nextOffset = 0f;

        for (int i = 0; i < _knots.Length - 1; i++)
        {
            BezierDefinition def = TransformBezierKnot.GetDefinition(_knots[i], _knots[i + 1]);
            Bezier curve = new Bezier(def);
            Vector3[] points = curve.GetEquallySpacedPoints(segmentLength, out float remainingDistance, nextOffset);
            nextOffset = segmentLength - remainingDistance;
            path.AddRange(points);
        }
        return path;
    }
}

[System.Serializable]
public class TransformBezierKnot
{
    public Transform transform;
    public float handleSize;

    public Vector3 Position => transform ? transform.position : Vector3.zero;
    public Vector3 Forwards => transform ? transform.forward : Vector3.forward;
    public Vector3 ForwardsHandle => Position + Forwards * handleSize;
    public Vector3 BackwardsHandle => Position - Forwards * handleSize;

    public TransformBezierKnot(Transform transform, float handleSize)
    {
        this.transform = transform;
        this.handleSize = handleSize;
    }

    public static BezierDefinition GetDefinition(TransformBezierKnot from, TransformBezierKnot to)
    {
        Vector3 p0 = from.Position;
        Vector3 p1 = from.ForwardsHandle;

        Vector3 p2 = to.BackwardsHandle;
        Vector3 p3 = to.Position;

        return new BezierDefinition(p0, p1, p2, p3);
    }
}