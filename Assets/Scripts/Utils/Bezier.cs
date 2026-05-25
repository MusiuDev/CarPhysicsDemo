using System.Collections.Generic;
using UnityEngine;
using System;

public static class BezierSpline
{
    public static SplinePath GetFullPath(IBezierKnot[] knots, float segmentLength, float linearOffset = 0f, int bezierSegments = 16)
    {
        return GetFullPathWithOffset(knots, segmentLength, 0, Vector3.zero, linearOffset, bezierSegments);
    }

    public static SplinePath GetFullPathWithOffset(IBezierKnot[] knots, float segmentLength, float offset, Vector3 upAxis, float linearOffset = 0f, int bezierSegments = 16)
    {
        if (knots == null || knots.Length == 0) return new SplinePath(Vector3.zero);
        if (knots.Length == 1) return new SplinePath(knots[0].Position);

        if (segmentLength <= 0.01f)
        {
            Debug.LogWarning("Segment Length is too small to draw. It would create performance issues.");
            return new SplinePath(knots[0].Position, knots[^1].Position);
        }

        SplinePath path = new SplinePath();
        float nextOffset = linearOffset;

        float remainingDistance = 0f;
        for (int i = 0; i < knots.Length - 1; i++)
        {
            if (knots[i] == null || knots[i + 1] == null)
            {
                Debug.LogWarning($"Invalid knot at {i} index");
            }
            BezierDefinition def = new BezierDefinition(knots[i], knots[i + 1], bezierSegments);
            Bezier curve = new Bezier(def);

            Vector3[] points = curve.GetRegularSegmentsByDistanceWithOffset(segmentLength, offset, upAxis, out remainingDistance, nextOffset);
            nextOffset = segmentLength - remainingDistance;
            path.pathPoints.AddRange(points);
        }

        path.remainingDistance = remainingDistance;
        return path;
    }
}

[System.Serializable]
public class BezierDefinition
{
    // Bernstein Matrix
    public static readonly Matrix4x4 M = new Matrix4x4(
        new Vector4(1, 0, 0, 0),
        new Vector4(-3, 3, 0, 0),
        new Vector4(3, -6, 3, 0),
        new Vector4(-1, 3, -3, 1)
    );

    public Vector3 p0, p1, p2, p3;
    public int segments;
    public Matrix4x4 matrix;
    // Cached coefficient matrix
    public Matrix4x4 GM;

    public BezierDefinition(IBezierKnot knot_a, IBezierKnot knot_b, int segments = 16)
        : this(knot_a.Position, knot_a.ForwardHandlePosition, knot_b.BackwardsHandlePosition, knot_b.Position, segments) { }

    public BezierDefinition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments = 16)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.segments = segments;
        matrix = new Matrix4x4(p0, p1, p2, p3);
        GM = matrix * M;
    }
}

public class Bezier
{
    private float[] MainLUT;
    public BezierDefinition _def;

    public Bezier(BezierDefinition def)
    {
        _def = def;
        MainLUT = MathUtils.GetLUT(Sample, def.segments);
    }

    private static Vector4 TVec(float t)
    {
        return new Vector4(1, t, t * t, t * t * t);
    }

    private static Vector4 TVecD(float t)
    {
        return new Vector4(0, 1, 2 * t, 3 * t * t);
    }

    private static Vector4 TVecD2(float t)
    {
        return new Vector4(0, 0, 2, 6 * t);
    }

    public Vector3 Sample(float t)
    {
        return Sample(t, 0f, Vector3.zero);
    }

    public Vector3 Sample(float t, float offset, Vector3 upAxis)
    {
        Vector3 p = _def.GM * TVec(t);

        if (offset == 0f) return p;

        Vector3 v = SampleDerivative(t).normalized;
        Vector3 n = Quaternion.AngleAxis(90f, upAxis) * v;

        return p + n * offset;
    }

    public Vector3 SampleDerivative(float t)
    {
        return _def.GM * TVecD(t);
    }

    public Vector3 SampleSecondDerivative(float t)
    {
        return _def.GM * TVecD2(t);
    }

    public Vector3[] GetRegularSegmentsByDistance(float segmentLength, out float remainingDistance, float linearOffset = 0)
    {
        return MathUtils.GetRegularSegmentsByDistance(MainLUT, Sample, segmentLength, out remainingDistance, linearOffset);
    }

    public Vector3[] GetRegularSegmentsByDistanceWithOffset(float segmentLength, float offset, Vector3 upAxis, out float remainingDistance, float linearOffset = 0)
    {
        Func<float, Vector3> sampleFunction = (t) => Sample(t, offset, upAxis);
        float[] offsetLUT = MathUtils.GetLUT(sampleFunction, _def.segments);
        return MathUtils.GetRegularSegmentsByDistance(offsetLUT, sampleFunction, segmentLength, out remainingDistance, linearOffset);
    }
}

public interface IBezierKnot
{
    Vector3 Position { get; }
    Vector3 ForwardHandlePosition { get; }
    Vector3 BackwardsHandlePosition { get; }
}

public struct BezierKnotSimple : IBezierKnot
{
    public Vector3 Position { get; set; }
    public Vector3 ForwardHandlePosition { get; set; }
    public Vector3 BackwardsHandlePosition { get; set; }
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

public struct Frenet
{
    public Vector3 t; //tangent
    public Vector3 n; //normal
    public Vector3 b; //binormal
    public float k; //curvature

    public Frenet(Vector3 t, Vector3 n, Vector3 b, float k)
    {
        this.t = t;
        this.n = n;
        this.b = b;
        this.k = k;
    }

    public Frenet(Vector3 t)
    {
        this.t = t;
        this.n = Vector3.zero;
        this.b = Vector3.zero;
        this.k = 0f;
    }
}