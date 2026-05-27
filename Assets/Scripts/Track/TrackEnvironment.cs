using System.Collections.Generic;
using UnityEngine;

public class TrackEnvironment : MonoBehaviour
{
    [SerializeField] private TrackManager _trackManager;
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private Transform _ground;
    [SerializeField] private Transform _groundCollider;
    [SerializeField] private GameObjectPoolable[] _cliffPrefabs;
    [SerializeField] private float _cliffsOffset;
    [SerializeField] private float _cliffsLinearSpacing;
    [SerializeField] private float _cliffKnotSize;
    [SerializeField] private Vector3 _firstKnotPosition;
    [SerializeField] private Transform _frontLimitsWall;
    [SerializeField] private Transform _backLimitsWall;

    private float _nextOffset_a = 0f;
    private float _nextOffset_b = 0f;
    private List<TransformBezierKnot> _knots = new List<TransformBezierKnot>();
    private Dictionary<TransformBezierKnot, List<GameObjectPoolable>> _knotsToCliffsDict = new Dictionary<TransformBezierKnot, List<GameObjectPoolable>>();

    void Awake()
    {
        TrackManager.OnTrackUpdated += HandleTrackUpdated;
        TrackManager.OnCheckpointGroupSpawned += HandleGroupSpawned;
        TrackManager.OnCheckpointGroupDespawned += HandleGroupDespawned;
        AddKnotAt(_firstKnotPosition, Quaternion.identity);
        AddKnotAt(Vector3.zero, Quaternion.identity);
        SetBackWallAt(_firstKnotPosition, Quaternion.identity);
        UpdateGroundCollider();
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

    void Update()
    {
        UpdateGroundCollider();
    }

    private void UpdateGroundCollider()
    {
        if (!_car || !_groundCollider) return;
        Vector3 groundPosition = _car.transform.position;
        groundPosition.y = _groundCollider.position.y;
        _groundCollider.position = groundPosition;
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

        _knots.Add(newKnot);

        if (_knots.Count > 1)
        {
            TransformBezierKnot prevKnot = _knots[^2];
            BezierDefinition def = new BezierDefinition(prevKnot, newKnot);
            Bezier curve = new Bezier(def);
            Vector3[] newCliffs_A = curve.GetRegularSegmentsByDistanceWithOffset(_cliffsLinearSpacing, _cliffsOffset, Vector3.up, out float remainingDistance_a, _nextOffset_a);
            Vector3[] newCliffs_B = curve.GetRegularSegmentsByDistanceWithOffset(_cliffsLinearSpacing, -_cliffsOffset, Vector3.up, out float remainingDistance_b, _nextOffset_b);
            _nextOffset_a = _cliffsLinearSpacing - remainingDistance_a;
            _nextOffset_b = _cliffsLinearSpacing - remainingDistance_b;
            List<GameObjectPoolable> cliffs = new List<GameObjectPoolable>();

            SpawnCliffsAt(newCliffs_A, cliffs);
            SpawnCliffsAt(newCliffs_B, cliffs);

            _knotsToCliffsDict.Add(prevKnot, cliffs);
        }
    }

    private void HandleGroupSpawned(CheckpointGroup group)
    {
        AddKnotAt(group.ExitPosition, group.ExitRotation);
        SetFrontWallAt(group.ExitPosition, group.ExitRotation);
    }

    private void SpawnCliffsAt(Vector3[] positions, List<GameObjectPoolable> cliffsList)
    {
        foreach (var pos in positions)
        {
            var cliff = DynamicPoolProvider.Get(_cliffPrefabs[Random.Range(0, _cliffPrefabs.Length - 1)]);
            cliff.transform.position = pos;
            cliff.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
            cliffsList.Add(cliff);
        }
    }

    private void HandleGroupDespawned(CheckpointGroup group)
    {
        var oldestKnot = _knots[0];
        _knots.RemoveAt(0);

        List<GameObjectPoolable> knotCliffs = _knotsToCliffsDict[oldestKnot];
        foreach (var item in knotCliffs)
        {
            if (item)
            {
                DynamicPoolProvider.Return(item);
            }
        }

        Destroy(oldestKnot.gameObject);
        if (_knots.Count > 1 && _knots[0] != null)
        {
            SetBackWallAt(_knots[0].transform.position, _knots[0].transform.rotation);
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
        GizmoUtils.DrawSplineFromKnots(_knots.ToArray(), _cliffsLinearSpacing);
        Gizmos.color = Color.magenta;
        GizmoUtils.DrawOffsetSplineFromKnots(_knots.ToArray(), _cliffsLinearSpacing, _cliffsOffset, Vector3.up);
        Gizmos.color = Color.red;
        GizmoUtils.DrawOffsetSplineFromKnots(_knots.ToArray(), _cliffsLinearSpacing, -_cliffsOffset, Vector3.up);
    }
}
