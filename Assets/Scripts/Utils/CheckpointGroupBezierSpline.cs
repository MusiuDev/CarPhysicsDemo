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
        
        Gizmos.color = Color.cyan;
        GizmoUtils.DrawSplineFromKnots(knots, segmentDistance);
    }
}