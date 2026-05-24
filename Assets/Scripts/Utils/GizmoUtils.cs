using UnityEngine;

public static class GizmoUtils
{
    public static void DrawBoundsGizmo(Transform bTransform)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Vector3 center = bTransform.position;
        center.y = 0;
        Quaternion rotation = Quaternion.Euler(0, bTransform.eulerAngles.y, 0);
        Vector3 scale = bTransform.localScale;
        scale.y = 0.01f;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, scale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = originalMatrix;
    }

    public static void DrawArrowGizmo(Vector3 from, Vector3 direction, float arrowSize = 3f)
    {
        Vector3 dir = direction.normalized;
        Vector3 to = from + direction.normalized * arrowSize;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(from, 0.2f);

        int arrowSections = 4;
        Vector3 arrowCap = Vector3.RotateTowards(-dir, dir, 30f * Mathf.Deg2Rad, 99f) * 0.6f;

        for (int i = 0; i < arrowSections; i++)
        {
            Vector3 newArrowCap = Quaternion.AngleAxis((360f / arrowSections) * i, dir) * arrowCap;
            Gizmos.DrawLine(to, to + newArrowCap);
        }
    }


    public static void DrawSplineFromKnots(IBezierKnot[] knots, float segmentDistance)
    {
        SplinePath path = BezierSpline.GetFullPath(knots, segmentDistance);

        if (path == null) return;
        if (path.pathPoints.Count <= 1) return;

        Vector3[] lineList = new Vector3[(path.pathPoints.Count - 1) * 2];
        for (int i = 0; i < path.pathPoints.Count - 1; i++)
        {
            lineList[2 * i] = path.pathPoints[i];
            lineList[2 * i + 1] = path.pathPoints[i + 1];
        }

        Gizmos.DrawLineList(lineList);
    }

    public static void DrawOffsetSplineFromKnots(IBezierKnot[] knots, float segmentDistance, float offset, Vector3 upAxis)
    {
        SplinePath path = BezierSpline.GetFullPathWithOffset(knots, segmentDistance, offset, upAxis, 0, 32);

        if (path == null) return;
        if (path.pathPoints.Count <= 1) return;

        Vector3[] lineList = new Vector3[(path.pathPoints.Count - 1) * 2];

        Gizmos.DrawSphere(path.pathPoints[0], segmentDistance * 0.25f);
        for (int i = 0; i < path.pathPoints.Count - 1; i++)
        {
            lineList[2 * i] = path.pathPoints[i];
            lineList[2 * i + 1] = path.pathPoints[i + 1];

            Gizmos.DrawSphere(path.pathPoints[i + 1], segmentDistance * 0.25f);
        }

        Gizmos.DrawLineList(lineList);
    }

    public static void DrawRotatedRectangle(RotatedRectangle rect, float y)
    {
        Vector3[] worldCorners = rect.GetCornersXZ(y);
        for (int i = 0; i < worldCorners.Length; i++)
        {
            Gizmos.DrawSphere(worldCorners[i], 0.25f);
            if (i < worldCorners.Length - 1)
            {
                Gizmos.DrawLine(worldCorners[i], worldCorners[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(worldCorners[i], worldCorners[0]);
            }
        }


        Color orgColor = Gizmos.color;
        Gizmos.DrawSphere(rect.center.ToXZ(), 0.5f);
        Gizmos.color = new Color(0.000f, 1.000f, 0.000f, 0.5f);
        GizmoUtils.DrawArrowGizmo(rect.center.ToXZ(), rect.Up.ToXZ());
        Gizmos.color = new Color(1.000f, 0.000f, 0.000f, 0.5f);
        GizmoUtils.DrawArrowGizmo(rect.center.ToXZ(), rect.Right.ToXZ());
        Gizmos.color = orgColor;
    }

    public static void DrawCircle(Vector3 position, Vector3 normal, float radius, int segments = 16)
    {
        normal = normal.normalized;

        Vector3 currentDirection = Vector3.RotateTowards(normal, -normal, 90f * Mathf.Deg2Rad, 999f) * radius;
        Vector3[] lines = new Vector3[segments * 2];
        float angleSegment = (360f / segments);
        for (int i = 0; i < segments; i++)
        {
            lines[i * 2] = position + currentDirection;
            currentDirection = Quaternion.AngleAxis(angleSegment, normal) * currentDirection;
            lines[i * 2 + 1] = position + currentDirection;
        }

        Gizmos.DrawLineList(lines);
    }
}
