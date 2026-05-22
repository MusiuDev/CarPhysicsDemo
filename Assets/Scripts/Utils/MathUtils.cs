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
}
