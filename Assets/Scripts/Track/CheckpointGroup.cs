using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroup : MonoBehaviour
{
    public delegate void CheckpointGroupEvent(CheckpointGroup group);
    public event CheckpointGroupEvent OnCheckpointGroupCleared;

    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private Transform _boundsTransform;

    public IReadOnlyCollection<Checkpoint> Checkpoints => _checkpoints;
    public Transform ExitPoint => _exitPoint;
    public Transform BoundsTransform => _boundsTransform;

    private int _currentCheckpointIndex;
    private Checkpoint _nextCheckpoint;
    public bool Completed { get; private set; }

    void Start()
    {
        ResetGroup();
    }

    public void ResetGroup()
    {
        SubscribeToCheckpoints();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].SetState(Checkpoint.CheckpointState.Blocked);
        }
        _currentCheckpointIndex = 0;
        Completed = false;
        UpdateNextCheckpoint();
    }

    void OnDestroy()
    {
        UnsubscribeFromCheckpoints();
    }


    private void HandleCheckpointCompleted(Checkpoint checkpoint)
    {
        if (checkpoint != _nextCheckpoint || Completed)
        {
            Debug.LogError("Invalid Checkpoint Completion. This should have never happened");
            return;
        }

        _currentCheckpointIndex++;
        if (_currentCheckpointIndex >= _checkpoints.Length)
        {
            MarkAsCompleted();
        }
        else
        {
            UpdateNextCheckpoint();
        }
    }

    private void UpdateNextCheckpoint()
    {
        _nextCheckpoint = _checkpoints[_currentCheckpointIndex];
        _nextCheckpoint.SetState(Checkpoint.CheckpointState.Active);
    }

    private void MarkAsCompleted()
    {
        Completed = true;
        OnCheckpointGroupCleared?.Invoke(this);
        UnsubscribeFromCheckpoints();
    }

    private void SubscribeToCheckpoints()
    {
        if (_checkpoints == null || _checkpoints.Length == 0) return;
        UnsubscribeFromCheckpoints();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].OnCheckpointCompleted += HandleCheckpointCompleted;
        }
    }

    private void UnsubscribeFromCheckpoints()
    {
        if (_checkpoints == null || _checkpoints.Length == 0) return;
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].OnCheckpointCompleted -= HandleCheckpointCompleted;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        GizmoUtils.DrawArrowGizmo(transform.position, transform.forward);

        if (_exitPoint)
        {
            Gizmos.color = Color.red;
            GizmoUtils.DrawArrowGizmo(_exitPoint.position, _exitPoint.forward);
        }

        if (_boundsTransform)
        {
            Gizmos.color = Color.orange;
            RotatedRectangle boundsRect = MathUtils.RectFromTransformXZ(_boundsTransform);
            GizmoUtils.DrawRotatedRectangle(boundsRect, transform.position.y);
        }
    }
}
