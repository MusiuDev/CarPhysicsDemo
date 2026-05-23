using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointGroup : MonoBehaviour
{
    public delegate void CheckpointGroupEvent(CheckpointGroup group);
    public static event CheckpointGroupEvent OnCheckpointGroupCleared;

    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private Transform _boundsTransform;

    public IReadOnlyCollection<Checkpoint> Checkpoints => _checkpoints;

    public Transform ExitPoint => _exitPoint;
    public Vector3 ExitPosition => _exitPoint ? _exitPoint.position : Vector3.zero;
    public float ExitAngle => _exitPoint ? _exitPoint.eulerAngles.y : 0f;

    public Transform BoundsTransform => _boundsTransform;

    private int _currentCheckpointIndex;
    private Checkpoint _nextCheckpoint;
    public bool Completed { get; private set; }

    public void Activate()
    {
        _currentCheckpointIndex = 0;
        InfiniteDriftGameManager.OnCarResetStarted -= HandleCarReset;
        InfiniteDriftGameManager.OnCarResetStarted += HandleCarReset;
        UpdateNextCheckpoint();
    }

    public void ResetGroup()
    {
        SubscribeToCheckpoints();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].SetState(Checkpoint.CheckpointState.Blocked);
        }
        _currentCheckpointIndex = -1;
        Completed = false;
        UpdateNextCheckpoint();
    }

    private void MarkAsCompleted()
    {
        Completed = true;
        OnCheckpointGroupCleared?.Invoke(this);
        InfiniteDriftGameManager.OnCarResetStarted -= HandleCarReset;
        UnsubscribeFromCheckpoints();
    }

    void OnDestroy()
    {
        UnsubscribeFromCheckpoints();
    }


    private void HandleCarReset()
    {
        _nextCheckpoint.HandleCarReset();
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
        if (_currentCheckpointIndex >= 0 && _currentCheckpointIndex < _checkpoints.Length)
        {
            _nextCheckpoint = _checkpoints[_currentCheckpointIndex];
            _nextCheckpoint.SetState(Checkpoint.CheckpointState.Active);
        }
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

    public RotatedRectangle GetCurrentBounds()
    {
        return MathUtils.RectFromTransformXZ(_boundsTransform);
    }

    public RotatedRectangle GetBoundsAt(Vector3 position, float rotation, bool flipped)
    {
        float flippedSign = flipped ? -1 : 1;
        Matrix4x4 trs = Matrix4x4.TRS(position, Quaternion.Euler(0, rotation, 0), new Vector3(flippedSign, 1, 1));

        Vector2 pos = trs.MultiplyPoint(_boundsTransform.localPosition).ToXY();
        float rot = rotation + (flippedSign * _boundsTransform.localEulerAngles.y);

        return new RotatedRectangle(pos, _boundsTransform.localScale.ToXY(), rot);
    }

    public TrackChainLink GetTrackChainLinkAt(Vector3 position, float rotation, bool flipped)
    {
        float flippedSign = flipped ? -1 : 1;
        Matrix4x4 trs = Matrix4x4.TRS(position, Quaternion.Euler(0, rotation, 0), new Vector3(flippedSign, 1, 1));

        Vector3 entryPosition = position;
        Vector3 exitPosition = trs.MultiplyPoint(this.ExitPosition);
        float exitAngle = rotation + (flippedSign * this.ExitAngle);

        Vector2 boundsPos = trs.MultiplyPoint(_boundsTransform.localPosition).ToXY();
        float boundsRotation = rotation + (flippedSign * _boundsTransform.localEulerAngles.y);
        Vector3 boundsScale = _boundsTransform.localScale.ToXY();

        RotatedRectangle transformedBounds = new RotatedRectangle(boundsPos, boundsScale, boundsRotation);

        return new TrackChainLink(
            entryPosition,
            exitPosition,
            exitAngle,
            transformedBounds
        );
    }

    public void Flip()
    {
        IFlippableObject[] childFlippables = gameObject.GetComponentsInChildren<IFlippableObject>();
        foreach (var item in childFlippables)
        {
            item.Flip();
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