using System.Collections.Generic;
using UnityEngine;

public static class RectangleIntersection
{
    public static bool CheckIntersection(RotatedRectangle rect_a, RotatedRectangle rect_b)
    {
        Vector2[] corners_a = rect_a.GetCorners();
        Vector2[] corners_b = rect_b.GetCorners();

        Vector2[] axes = new Vector2[4]
        {
            rect_a.Up,
            rect_a.Right,
            rect_b.Up,
            rect_b.Right
        };

        foreach (var axis in axes)
        {
            MinMax a_proj = GetMinMaxOnAxis(corners_a, axis);
            MinMax b_proj = GetMinMaxOnAxis(corners_b, axis);

            if (!a_proj.Overlaps(b_proj))
            {
                return false;
            }
        }
        return true;
    }

    private static MinMax GetMinMaxOnAxis(Vector2[] corners, Vector2 axis)
    {
        MinMax result = MinMax.Inf;
        axis.Normalize();
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 corner = corners[i];
            float v = Vector2.Dot(corner, axis);
            result.Encapsulate(v);
        }
        return result;
    }
}

public struct RotatedRectangle
{
    public Vector2 center;
    public Vector2 size;
    public float rotation;
    public float RotationRad => -rotation * Mathf.Deg2Rad;

    private float SinRot => Mathf.Sin(RotationRad);
    private float CosRot => Mathf.Cos(RotationRad);

    public RotatedRectangle(Vector2 center, Vector2 size, float rotation)
    {
        this.center = center;
        this.size = size;
        this.rotation = rotation;
    }

    public Vector2[] GetCorners()
    {
        Vector2[] corners = new Vector2[4];

        float w = size.x * 0.5f;
        float h = size.y * 0.5f;

        float cx = center.x;
        float cy = center.y;

        float xw = w * CosRot;
        float xh = h * SinRot;

        float yw = w * SinRot;
        float yh = h * CosRot;

        corners[0] = new Vector2(cx + xw - xh, cy + yw + yh);
        corners[1] = new Vector2(cx - xw - xh, cy - yw + yh);
        corners[2] = new Vector2(cx - xw + xh, cy - yw - yh);
        corners[3] = new Vector2(cx + xw + xh, cy + yw - yh);

        return corners;
    }

    public Vector2 Up => new Vector2(-SinRot, CosRot);
    public Vector2 Right => new Vector2(CosRot, SinRot);
}

public struct MinMax
{
    public float min;
    public float max;

    public MinMax(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public void Encapsulate(float v)
    {
        if (v < min) min = v;
        if (v > max) max = v;
    }

    public bool Overlaps(MinMax other)
    {
        return
            this.Contains(other.min) ||
            this.Contains(other.max) ||
            other.Contains(this.min) ||
            other.Contains(this.max);
    }

    public bool Contains(float f)
    {
        return f.Between(min, max);
    }

    public static MinMax Inf => new MinMax(float.MaxValue, float.MinValue);

}
