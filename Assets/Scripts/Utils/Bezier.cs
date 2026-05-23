using System.Collections.Generic;
using UnityEngine;

public class Bezier
{
    private BezierDefinition _def;
    private int _segments = 16;
    private Vector3 _coef_a;
    private Vector3 _coef_b;
    private Vector3 _coef_c;
    private float[] LUT;
    public float ArcLength => (LUT != null && LUT.Length > 0) ? LUT[^1] : 0;

    public Bezier(BezierDefinition definition, int segments = 16)
    {
        _segments = segments;
        SetPoints(definition);
    }

    public Bezier(IBezierKnot from, IBezierKnot to, int segments = 16)
    {
        _segments = segments;
        Vector3 p0 = from.Position;
        Vector3 p1 = from.ForwardHandlePosition;
        Vector3 p2 = to.BackwardsHandlePosition;
        Vector3 p3 = to.Position;
        SetPoints(new BezierDefinition(p0, p1, p2, p3));
    }

    public void SetPoints(BezierDefinition definition)
    {
        _def = definition;
        UpdateCoefficients();
        UpdateLUT();
    }

    private void UpdateCoefficients()
    {
        _coef_a = -3 * _def.p0 + 3 * _def.p1;
        _coef_b = 3 * _def.p0 - 6 * _def.p1 + 3 * _def.p2;
        _coef_c = -_def.p0 + 3 * _def.p1 - 3 * _def.p2 + _def.p3;
    }

    private void UpdateLUT()
    {
        LUT = new float[_segments + 1];
        LUT[0] = 0;

        Vector3[] positionsByT = new Vector3[_segments + 1];
        if (positionsByT.Length == 0 || _segments == 0)
        {
            Debug.Log("Why?");
        }

        positionsByT[0] = _def.p0;

        float segmentSize = 1f / _segments;
        for (int i = 1; i <= _segments; i++)
        {
            float t = segmentSize * i;
            Vector3 newSample = Sample(t);
            positionsByT[i] = newSample;

            float segmentDistance = Vector3.Distance(positionsByT[i - 1], newSample);
            LUT[i] = LUT[i - 1] + segmentDistance;
        }
    }

    public Vector3[] GetEquallySpacedPoints(float segmentDistance, float offset = 0f)
    {
        return GetEquallySpacedPoints(segmentDistance, out _, offset);
    }

    public Vector3[] GetEquallySpacedPoints(float segmentDistance, out float remainingDistance, float offset = 0f)
    {
        if (ArcLength < segmentDistance)
        {
            remainingDistance = segmentDistance - ArcLength; //TODO: check if this is correct
            return new Vector3[0];
        }

        int pointCount = Mathf.FloorToInt((ArcLength - offset) / segmentDistance);
        Vector3[] points = new Vector3[pointCount + 1];

        remainingDistance = (ArcLength - offset) - (pointCount * segmentDistance);
        for (int i = 0; i < points.Length; i++)
        {
            float t = DistToT(segmentDistance * i + offset);
            points[i] = Sample(t);
        }

        return points;
    }

    public float DistToT(float distance)
    {
        int n = LUT.Length;

        if (distance.Within(0, ArcLength))
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (distance.Between(LUT[i], LUT[i + 1]))
                {
                    return distance.Remap(
                        LUT[i],
                        LUT[i + 1],
                        i / (n - 1f),
                        (i + 1) / (n - 1f)
                    );
                }
            }
        }
        return distance / ArcLength;
    }

    public Vector3 Sample(float t)
    {
        //here for legibility
        float t2 = t * t;
        float t3 = t * t * t;

        return _def.p0 + t * _coef_a + t2 * _coef_b + t3 * _coef_c;
    }
}

[System.Serializable]
public struct BezierDefinition
{
    public Vector3 p0, p1, p2, p3;

    public BezierDefinition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }
}

public static class BezierSpline
{
    public static SplinePath GetFullPath(IBezierKnot[] knots, float segmentLength, float offset = 0f, int bezierSegments = 16)
    {

        if (knots == null || knots.Length == 0) return new SplinePath(Vector3.zero);
        if (knots.Length == 1) return new SplinePath(knots[0].Position);

        if (segmentLength <= 0.01f)
        {
            Debug.LogWarning("Segment Length is too small to draw. It would create performance issues.");
            return new SplinePath(knots[0].Position, knots[^1].Position);
        }

        SplinePath path = new SplinePath();
        float nextOffset = offset;

        float remainingDistance = 0f;
        for (int i = 0; i < knots.Length - 1; i++)
        {
            Bezier curve = new Bezier(knots[i], knots[i + 1], bezierSegments);
            Vector3[] points = curve.GetEquallySpacedPoints(segmentLength, out remainingDistance, nextOffset);
            nextOffset = segmentLength - remainingDistance;
            path.pathPoints.AddRange(points);
        }
        
        path.remainingDistance = remainingDistance;
        return path;
    }
}

public interface IBezierKnot
{
    Vector3 Position { get; }
    Vector3 ForwardHandlePosition { get; }
    Vector3 BackwardsHandlePosition { get; }
}
public class SplinePath
{
    public List<Vector3> pathPoints;
    public float remainingDistance;

    public SplinePath()
    {
        pathPoints = new List<Vector3>();
    }

    public SplinePath(params Vector3[] points)
    {
        pathPoints = new List<Vector3>(points);
    }
}