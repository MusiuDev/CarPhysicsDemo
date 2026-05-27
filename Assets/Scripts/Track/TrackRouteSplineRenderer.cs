using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackRouteSplineRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineSegmentDistance = 1f;
    [SerializeField] private int _bezierSegments = 16;
    [SerializeField] private float _firstPointDistance = 100f;


    private Dictionary<CheckpointGroup, SplinePath> _groupToPath = new Dictionary<CheckpointGroup, SplinePath>();
    private List<SplinePath> _activePaths = new List<SplinePath>();
    private List<CheckpointGroup> _activeGroups = new List<CheckpointGroup>();
    private int _fullPathCount = 0;
    private float _nextOffset = 0f;

    void Awake()
    {
        TrackManager.OnCheckpointGroupSpawned += HandleGroupSpawned;
        TrackManager.OnCheckpointGroupDespawned += HandleGroupDespawned;
        TrackManager.OnTrackUpdated += UpdateLineRenderer; //This one only gets called after bulk updates, so we only update the renderer here.
    }

    void OnDestroy()
    {
        TrackManager.OnCheckpointGroupSpawned -= HandleGroupSpawned;
        TrackManager.OnCheckpointGroupDespawned -= HandleGroupDespawned;
        TrackManager.OnTrackUpdated -= UpdateLineRenderer;
    }

    private void HandleGroupSpawned(CheckpointGroup group)
    {
        CheckpointGroupBezierSpline groupSpline = group.GetComponent<CheckpointGroupBezierSpline>();
        if (!groupSpline) return;

        SplinePath path = BezierSpline.GetFullPath(groupSpline.knots, _lineSegmentDistance, _nextOffset, _bezierSegments);
        _nextOffset = _lineSegmentDistance - path.remainingDistance;

        _groupToPath.Add(group, path);
        _activeGroups.Add(group);
        _activePaths.Add(path);
        _fullPathCount += path.pathPoints.Count;
    }

    private void HandleGroupDespawned(CheckpointGroup group)
    {
        if (!_groupToPath.ContainsKey(group)) return;

        SplinePath path = _groupToPath[group];
        if (path == null) return;

        _activePaths.Remove(path);
        _groupToPath.Remove(group);
        _activeGroups.Remove(group);

        _fullPathCount -= path.pathPoints.Count;
    }


    private void UpdateLineRenderer()
    {
        if (_activePaths == null || _activePaths.Count == 0) return;
        if (_activeGroups == null || _activeGroups.Count == 0) return;

        Vector3[] fullPath = new Vector3[_fullPathCount + 1];
        Vector3 firstPoint = _activeGroups[0].transform.position - _activeGroups[0].transform.forward * _firstPointDistance;
        fullPath[0] = Swizzle(firstPoint);

        int index = 1;
        foreach (var path in _activePaths)
        {
            foreach (var point in path.pathPoints)
            {
                fullPath[index] = Swizzle(point);
                index++;
            }
        }

        _lineRenderer.positionCount = fullPath.Length;
        _lineRenderer.SetPositions(fullPath);
    }

    private Vector3 Swizzle(Vector3 pos)
    {
        return new Vector3(pos.x, pos.z, pos.y);
    }
}
