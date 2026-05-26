using System.Collections;
using UnityEngine;

public class InfiniteDriftGameManager : GameManager
{
    [SerializeField] private float _resetCarDelay;
    [SerializeField] private float _enableInputDelay;

    protected override void HandleAwake()
    {
        CheckpointGroup.OnCheckpointGroupCleared += HandleCheckpointGroupCleared;
        _carCollision.OnCarCollisionWithObstacle += HandleCarCollision;
        _carStuck.OnCarStuck += HandleCarStuck;
        _safeRevivePosition = _car.transform.position;
        _safeReviveRotation = _car.transform.rotation;
        _gameActive = false;
    }

    protected override void HandleDestroy()
    {
        CheckpointGroup.OnCheckpointGroupCleared -= HandleCheckpointGroupCleared;
        if (_carCollision) _carCollision.OnCarCollisionWithObstacle -= HandleCarCollision;
        if (_carStuck) _carStuck.OnCarStuck += HandleCarStuck;
        _gameActive = false;
        RaiseOnGameStopped();
    }

    protected override void HandleTransitionComplete()
    {
        StartCoroutine(EnableInputAfterDelay(_enableInputDelay));
        _gameActive = true;
        RaiseOnGameStarted();
    }

    private void HandleCarCollision(Collision col)
    {
        RequestResetcar(_safeRevivePosition, _safeReviveRotation, _resetCarDelay);
    }

    private void HandleCarStuck()
    {
        RequestResetcar(_safeRevivePosition, _safeReviveRotation, _resetCarDelay);
    }

    protected override void CarResetStarted()
    {
        _gameActive = false;
        _inputController.InputEnabled = false;
    }

    protected override void CarResetCompleted()
    {
        StartCoroutine(EnableInputAfterDelay(_enableInputDelay));
    }

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _gameActive = true;
        _inputController.InputEnabled = true;
    }

    private void HandleCheckpointGroupCleared(CheckpointGroup group)
    {
        _safeRevivePosition = group.ExitPosition;
        _safeReviveRotation = group.ExitRotation;
    }
}
