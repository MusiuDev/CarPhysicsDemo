using System.Collections;
using UnityEngine;

public class InfiniteDriftGameManager : GameManager
{
    [SerializeField] private float _resetCarDelay;
    [SerializeField] private float _enableInputDelay;

    private Vector3 _revivePosition;
    private Quaternion _reviveRotation;

    protected override void HandleAwake()
    {
        CheckpointGroup.OnCheckpointGroupCleared += HandleCheckpointGroupCleared;
        _carCollision.OnCarCollisionWithObstacle += HandleCarCollision;
        _carStuck.OnCarStuck += HandleCarStuck;
        _revivePosition = _car.transform.position;
        _reviveRotation = _car.transform.rotation;
        _gameActive = false;
    }

    protected override void HandleDestroy()
    {
        CheckpointGroup.OnCheckpointGroupCleared -= HandleCheckpointGroupCleared;
        if (_carCollision) _carCollision.OnCarCollisionWithObstacle -= HandleCarCollision;
        if (_carStuck) _carStuck.OnCarStuck += HandleCarStuck;
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
        RequestResetcar(_revivePosition, _reviveRotation, _resetCarDelay);
    }

    private void HandleCarStuck()
    {
        RequestResetcar(_revivePosition, _reviveRotation, _resetCarDelay);
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
        _revivePosition = group.ExitPosition;
        _reviveRotation = group.ExitRotation;
    }
}
