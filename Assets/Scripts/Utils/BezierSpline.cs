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