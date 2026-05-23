using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackRouteSplineRenderer : MonoBehaviour
{
    [SerializeField] private TrackManager _trackManager;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineSegmentDistance = 1f;

    void Awake()
    {
        _trackManager.OnTrackUpdated += HandleTrackUpdated;
    }

    private void HandleTrackUpdated()
    {
        var groups = _trackManager.ActiveGroups;
        List<IBezierKnot> allKnots = new();
        foreach (var group in groups)
        {
            CheckpointGroupBezierSpline groupSpline = group.GetComponent<CheckpointGroupBezierSpline>();
            if (groupSpline)
            {
                allKnots.AddRange(groupSpline.knots);
                allKnots.RemoveAt(allKnots.Count - 1); //don't add the last one so the spline doesn't double up.
            }
        }



        if (_lineRenderer)
        {
            Vector3[] path = BezierSpline.GetFullPath(allKnots.ToArray(), _lineSegmentDistance).ToArray();
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = Swizzle(path[i]);
            }
            _lineRenderer.positionCount = path.Length;
            _lineRenderer.SetPositions(path);
        }
    }

    private Vector3 Swizzle(Vector3 pos)
    {
        return new Vector3(pos.x, pos.z, pos.y);
    }
}
