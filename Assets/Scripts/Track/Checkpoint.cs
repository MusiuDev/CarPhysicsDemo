using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public delegate void CheckpointEvent(Checkpoint checkpoint);
    public event CheckpointEvent OnCheckpointCompleted;

    [SerializeField] private Animator _gateAnimator;
    [SerializeField] private GameObject _colliderContainer;

    private CheckpointState _currentState = CheckpointState.Unset;
    private bool _isPlayerIn = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerBody")) return;//TODO: Change this to something better.
        Vector3 toPlayer = other.transform.position - this.transform.position;
        bool enteredFromTheBack = Vector3.Dot(-this.transform.forward, toPlayer) > 0;
        if (enteredFromTheBack)
        {
            _isPlayerIn = true;
        }

        if (_currentState != CheckpointState.Active) return;
        SetState(CheckpointState.Open);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerBody")) return;//TODO: Change this to something better.

        _isPlayerIn = false;

        if (_currentState != CheckpointState.Open) return;

        Vector3 toPlayer = other.transform.position - this.transform.position;
        bool exitFromFromt = Vector3.Dot(this.transform.forward, toPlayer) > 0;
        if (exitFromFromt)
        {
            SetState(CheckpointState.Completed);
            OnCheckpointCompleted?.Invoke(this);
        }
        else
        {
            SetState(CheckpointState.Active);
        }

    }

    public void SetState(CheckpointState newState)
    {
        if (_currentState == newState) return;

        //If the player is already in when the checkpoint gets activated, open the gate instantly. 
        if (_isPlayerIn && newState == CheckpointState.Active)
        {
            newState = CheckpointState.Open;
        }

        _currentState = newState;
        _colliderContainer.SetActive((int)_currentState < (int)CheckpointState.Open);
        _gateAnimator.SetInteger("State", (int)_currentState);
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
