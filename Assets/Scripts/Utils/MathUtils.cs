using UnityEngine;

public static class MathUtils
{
    //-- Math Extensions
    public static bool Between(this float v, float min, float max) => v >= min && v <= max;
    public static bool Within(this float v, float min, float max) => v > min && v < max;
    public static Vector3 ToXZ(this Vector2 v, float y = 0) => new Vector3(v.x, y, v.y);
    public static Vector2 ToXY(this Vector3 v) => new Vector2(v.x, v.z);
    public static float Remap(this float value, float iMin, float iMax, float oMin, float oMax) => Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, value));


    //-- Rotated Rectangle Extensions
    public static RotatedRectangle RectFromTransformXZ(Transform transform)
    {
        if (!transform) return RotatedRectangle.Zero;
        return new RotatedRectangle(transform.position.ToXY(), transform.localScale.ToXY(), transform.eulerAngles.y);
    }

    public static Vector3[] GetCornersXZ(this RotatedRectangle rect, float y = 0)
    {
        Vector2[] corners = rect.GetCorners();
        Vector3[] xzCorners = new Vector3[4];

        for (int i = 0; i < 4; i++)
        {
            xzCorners[i] = corners[i].ToXZ(y);
        }
        return xzCorners;
    }


    //-- Bezier related helper functions
    public static float DistToT(float distance, float[] LUT)
    {
        int n = LUT.Length;
        float ArcLength = LUT[^1];

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

    public static float[] GetLUT(System.Func<float, Vector3> sampleFunc, int segments)
    {
        if (segments == 0)
        {
            Debug.LogError("Trying to generate LUT for zero segments. Creating empty LUT");
            return new float[] { 0, 1 };
        }

        float[] LUT = new float[segments + 1];
        LUT[0] = 0;

        Vector3[] positionsByT = new Vector3[segments + 1];

        positionsByT[0] = sampleFunc(0);

        float segmentSize = 1f / segments;
        for (int i = 1; i <= segments; i++)
        {
            float t = segmentSize * i;
            Vector3 newSample = sampleFunc(t);
            positionsByT[i] = newSample;

            float segmentDistance = Vector3.Distance(positionsByT[i - 1], newSample);
            LUT[i] = LUT[i - 1] + segmentDistance;
        }

        return LUT;
    }

    public static Vector3[] GetRegularSegmentsByDistance(float[] LUT, System.Func<float, Vector3> sampleFunc, float segmentDistance, float offset = 0f)
    {
        return GetRegularSegmentsByDistance(LUT, sampleFunc, segmentDistance, out _, offset);
    }

    public static Vector3[] GetRegularSegmentsByDistance(float[] LUT, System.Func<float, Vector3> sampleFunc, float segmentDistance, out float remainingDistance, float linearOffset = 0f)
    {
        float totalLength = LUT[^1];
        if (totalLength < segmentDistance)
        {
            remainingDistance = segmentDistance - totalLength; //TODO: check if this is correct
            return new Vector3[0];
        }

        int pointCount = Mathf.FloorToInt((totalLength - linearOffset) / segmentDistance);
        Vector3[] points = new Vector3[pointCount + 1];

        remainingDistance = (totalLength - linearOffset) - (pointCount * segmentDistance);
        for (int i = 0; i < points.Length; i++)
        {
            float t = DistToT(segmentDistance * i + linearOffset, LUT);
            points[i] = sampleFunc(t);
        }

        return points;
    }
}
