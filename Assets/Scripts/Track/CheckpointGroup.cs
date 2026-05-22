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
        DrawArrowGizmo(transform.position, transform.forward);

        if (_exitPoint)
        {
            Gizmos.color = Color.red;
            DrawArrowGizmo(_exitPoint.position, _exitPoint.forward);
        }

        if (_boundsTransform)
        {
            Gizmos.color = Color.orange;
            //Gizmos.DrawWireSphere(_boundsTransform.position, _boundsTransform.localScale.x);
            DrawBoundsGizmo();
        }
    }

    private void DrawArrowGizmo(Vector3 from, Vector3 direction, float arrowSize = 3f)
    {
        Vector3 to = from + direction.normalized * arrowSize;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(from, 0.2f);

        int arrowSections = 4;
        Vector3 arrowCap = Vector3.RotateTowards(-direction, direction, 30f * Mathf.Deg2Rad, 99f) * 0.6f;

        for (int i = 0; i < arrowSections; i++)
        {
            Vector3 newArrowCap = Quaternion.AngleAxis((360f / arrowSections) * i, direction) * arrowCap;
            Gizmos.DrawLine(to, to + newArrowCap);
        }
    }

    private void DrawBoundsGizmo()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Vector3 center = _boundsTransform.position;
        center.y = 0;
        Quaternion rotation = Quaternion.Euler(0, _boundsTransform.eulerAngles.y, 0);
        Vector3 scale = _boundsTransform.localScale;
        scale.y = 0.01f;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, scale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = originalMatrix;
    }
}