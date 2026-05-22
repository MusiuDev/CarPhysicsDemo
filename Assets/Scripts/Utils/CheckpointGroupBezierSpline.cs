using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroupBezierSpline : MonoBehaviour
{
    public TransformBezierKnot[] knots;
    public float segmentDistance = 0.5f;

    void OnDrawGizmos()
    {
        if (knots == null) return;
        if (knots.Length <= 1) return;

        var path = BezierSpline.GetFullPath(knots, segmentDistance);

        if (path == null) return;
        if (path.Count <= 1) return;

        Vector3[] lineList = new Vector3[(path.Count - 1) * 2];
        for (int i = 0; i < path.Count - 1; i++)
        {
            lineList[2 * i] = path[i];
            lineList[2 * i + 1] = path[i + 1];
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLineList(lineList);
    }
}