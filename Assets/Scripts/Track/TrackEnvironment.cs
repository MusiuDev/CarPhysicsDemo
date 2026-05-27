using System.Collections.Generic;
using UnityEngine;

public class TrackEnvironment : MonoBehaviour
{
    [SerializeField] private TrackManager _trackManager;
    [SerializeField] private Transform _ground;
    [SerializeField] private GameObject[] _cliffPrefabs;
    [SerializeField] private float _cliffsOffset;
    [SerializeField] private float _cliffsLinearSpacing;
    [SerializeField] private float _cliffKnotSize;
    [SerializeField] private Vector3 _firstKnotPosition;
    [SerializeField] private Transform _frontLimitsWall;
    [SerializeField] private Transform _backLimitsWall;

    private float _nextOffset_a = 0f;
    private float _nextOffset_b = 0f;
    private List<TransformBezierKnot> knots = new List<TransformBezierKnot>();

    void Awake()
    {
        TrackManager.OnTrackUpdated += HandleTrackUpdated;
        TrackManager.OnCheckpointGroupSpawned += HandleGroupSpawned;
        TrackManager.OnCheckpointGroupDespawned += HandleGroupDespawned;
        AddKnotAt(_firstKnotPosition, Quaternion.identity);
        AddKnotAt(Vector3.zero, Quaternion.identity);
        SetBackWallAt(_firstKnotPosition, Quaternion.identity);
    }

    void OnDestroy()
    {
        TrackManager.OnTrackUpdated -= HandleTrackUpdated;
        TrackManager.OnCheckpointGroupSpawned -= HandleGroupSpawned;
        TrackManager.OnCheckpointGroupDespawned -= HandleGroupDespawned;
    }

    void Start()
    {

    }

    private void AddKnotAt(Vector3 pos, Quaternion rotation)
    {
        GameObject go = new GameObject("Group Knot", typeof(TransformBezierKnot));
        go.transform.SetParent(this.transform);
        go.transform.position = pos;
        go.transform.rotation = rotation;

        TransformBezierKnot newKnot = go.GetComponent<TransformBezierKnot>();

        newKnot.forwardsHandleSize = _cliffKnotSize;
        newKnot.backwardsHandleSize = _cliffKnotSize;

        knots.Add(newKnot);

        if (knots.Count > 1)
        {
            TransformBezierKnot prevKnot = knots[^2];
            BezierDefinition def = new BezierDefinition(prevKnot, newKnot);
            Bezier curve = new Bezier(def);
            Vector3[] newCliffs_A = curve.GetRegularSegmentsByDistanceWithOffset(_cliffsLinearSpacing, _cliffsOffset, Vector3.up, out float remainingDistance_a, _nextOffset_a);
            Vector3[] newCliffs_B = curve.GetRegularSegmentsByDistanceWithOffset(_cliffsLinearSpacing, -_cliffsOffset, Vector3.up, out float remainingDistance_b, _nextOffset_b);
            _nextOffset_a = _cliffsLinearSpacing - remainingDistance_a;
            _nextOffset_b = _cliffsLinearSpacing - remainingDistance_b;

            SpawnCliffsAt(newCliffs_A, prevKnot.transform);
            SpawnCliffsAt(newCliffs_B, prevKnot.transform);
        }
    }

    private void HandleGroupSpawned(CheckpointGroup group)
    {
        AddKnotAt(group.ExitPosition, group.ExitRotation);
        SetFrontWallAt(group.ExitPosition, group.ExitRotation);
    }

    private void SpawnCliffsAt(Vector3[] positions, Transform parent)
    {
        foreach (var pos in positions)
        {
            GameObject prefab = _cliffPrefabs[Random.Range(0, _cliffPrefabs.Length)];
            Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        }
    }

    private void HandleGroupDespawned(CheckpointGroup group)
    {
        var oldestKnot = knots[0];
        knots.RemoveAt(0);
        Destroy(oldestKnot.gameObject);
        if (knots.Count > 1 && knots[0] != null)
        {
            SetBackWallAt(knots[0].transform.position, knots[0].transform.rotation);
        }
    }

    private void HandleTrackUpdated()
    {
        if (!_ground) return;

        Vector3 first = _trackManager.CurrentChain.First.entryPosition;
        Vector3 last = _trackManager.CurrentChain.Last.exitPosition;

        Vector3 center = (first + last) * 0.5f;
        Vector3 direction = last - first;

        _ground.position = center;
        _ground.forward = direction.normalized;
    }

    private void SetBackWallAt(Vector3 position, Quaternion rotation)
    {
        if (!_backLimitsWall) return;
        _backLimitsWall.position = position;
        _backLimitsWall.rotation = rotation;
    }

    private void SetFrontWallAt(Vector3 position, Quaternion rotation)
    {
        if (!_frontLimitsWall) return;
        _frontLimitsWall.position = position;
        _frontLimitsWall.rotation = rotation;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        GizmoUtils.DrawSplineFromKnots(knots.ToArray(), _cliffsLinearSpacing);
        Gizmos.color = Color.magenta;
        GizmoUtils.DrawOffsetSplineFromKnots(knots.ToArray(), _cliffsLinearSpacing, _cliffsOffset, Vector3.up);
        Gizmos.color = Color.red;
        GizmoUtils.DrawOffsetSplineFromKnots(knots.ToArray(), _cliffsLinearSpacing, -_cliffsOffset, Vector3.up);
    }
}
