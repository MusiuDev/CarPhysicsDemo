using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroup : MonoBehaviour
{
    public delegate void CheckpointGroupEvent(CheckpointGroup group);
    public event CheckpointGroupEvent OnCheckpointGroupCleared;

    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform exitPoint;
    public BezierSpline spline;

    public IReadOnlyCollection<Checkpoint> Checkpoints => _checkpoints;
    private int _currentCheckpointIndex;
    private Checkpoint _nextCheckpoint;
    public bool Completed { get; private set; }

    void Start()
    {
        Reset();
    }

    public void Reset()
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
        StartCoroutine(WaitAndReset());
    }

    private void SubscribeToCheckpoints()
    {
        UnsubscribeFromCheckpoints();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].OnCheckpointCompleted += HandleCheckpointCompleted;
        }
    }

    private void UnsubscribeFromCheckpoints()
    {
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].OnCheckpointCompleted -= HandleCheckpointCompleted;
        }
    }

    IEnumerator WaitAndReset()
    {
        yield return new WaitForSeconds(2f);
        Reset();
    }
}