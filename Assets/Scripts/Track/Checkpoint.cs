using System.Collections;
using UnityEngine;

public abstract class Checkpoint : MonoBehaviour
{
    public delegate void CheckpointEvent(Checkpoint checkpoint);
    public event CheckpointEvent OnCheckpointCompleted;
    protected const string PLAYER_BODY_TAG = "PlayerBody";

    protected CheckpointState _currentState = CheckpointState.Unset;
    private bool _isPlayerIn = false;
    private bool _isResetting = false;

    void OnTriggerEnter(Collider other)
    {
        if (_isResetting) return;
        if (!other.CompareTag(PLAYER_BODY_TAG)) return;
        if (ProcessPlayerEnter(other.transform))
        {
            _isPlayerIn = true;
        }

        if (_currentState != CheckpointState.Active) return;
        SetState(CheckpointState.Open);
    }

    void OnTriggerExit(Collider other)
    {
        if (_isResetting) return;
        if (!other.CompareTag(PLAYER_BODY_TAG)) return;
        if (_currentState != CheckpointState.Open) return;

        bool _validExit = _isPlayerIn && ProcessPlayerExit(other.transform);

        _isPlayerIn = false;

        if (_validExit)
        {
            SetState(CheckpointState.Completed);
            OnCheckpointCompleted?.Invoke(this);
        }
        else
        {
            SetState(CheckpointState.Active);
        }
    }

    protected void NotifyCheckpointCompleted()
    {
        OnCheckpointCompleted?.Invoke(this);
    }

    protected abstract bool ProcessPlayerEnter(Transform playerTransform);
    protected abstract bool ProcessPlayerExit(Transform playerTransform);

    public void SetState(CheckpointState newState)
    {
        if (_currentState == newState) return;

        //If the player is already in when the checkpoint gets activated, set it as open immediately.
        if (_isPlayerIn && newState == CheckpointState.Active)
        {
            newState = CheckpointState.Open;
        }
        _currentState = newState;
        UpdateStatePresentation();
    }

    protected abstract void UpdateStatePresentation();

    public void HandleCarResetStart()
    {
        _isPlayerIn = false;
        _isResetting = true;
    }

    public void HandleCarResetcomplete()
    {
        _isResetting = false;
        if (_currentState == CheckpointState.Open)
        {
            SetState(CheckpointState.Active);
        }
    }

    public enum CheckpointState
    {
        Unset = -1,
        Blocked = 0,
        Active = 1,
        Open = 2,
        Completed = 3,
    }
}
