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

    public Bezier(BezierKnot from, BezierKnot to, int segments = 16)
    {
        _segments = segments;
        Vector3 p0 = from.position;
        Vector3 p1 = from.ForwardHandlePosition;
        Vector3 p2 = to.BackwardsHandlePosition;
        Vector3 p3 = to.position;
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

        if (distance > 0 && distance < ArcLength)
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (distance >= LUT[i] && distance <= LUT[i + 1])
                {
                    return Remap(
                        LUT[i],
                        LUT[i + 1],
                        i / (n - 1f),
                        (i + 1) / (n - 1f),
                        distance
                    );
                }
            }
        }
        return distance / ArcLength;
    }

    private float Remap(float iMin, float iMax, float oMin, float oMax, float value)
    {
        return Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, value));
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

[System.Serializable]
public class BezierKnot
{
    public Vector3 position;
    public Quaternion rotation;
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