using UnityEngine;

public class Bezier
{
    private Vector3 _p0, _p1, _p2, _p3;
    private int _segments = 16;

    private Vector3 _coef_a;
    private Vector3 _coef_b;
    private Vector3 _coef_c;

    private float[] LUT;
    public float ArcLength => (LUT != null && LUT.Length > 0) ? LUT[^1] : 0;

    public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments = 16)
    {
        _segments = segments;
        SetPoints(p0, p1, p2, p3);
    }

    public void SetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this._p0 = p0;
        this._p1 = p1;
        this._p2 = p2;
        this._p3 = p3;
        UpdateCoefficients();
        UpdateLUT();
    }

    private void UpdateCoefficients()
    {
        _coef_a = -3 * _p0 + 3 * _p1;
        _coef_b = 3 * _p0 - 6 * _p1 + 3 * _p2;
        _coef_c = -_p0 + 3 * _p1 - 3 * _p2 + _p3;
    }

    private void UpdateLUT()
    {
        LUT = new float[_segments + 1];
        LUT[0] = 0;

        Vector3[] positionsByT = new Vector3[_segments];
        positionsByT[0] = _p0;

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

    public Vector3[] GetEquallySpacedPoints(int count, float offset = 0f)
    {
        Vector3[] points = new Vector3[count];
        float distSegment = (1 / count) * ArcLength;

        for (int i = 0; i < count; i++)
        {
            float t = DistToT(distSegment * i + offset);
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

        return _p0 + t * _coef_a + t2 * _coef_b + t3 * _coef_c;
    }
}
