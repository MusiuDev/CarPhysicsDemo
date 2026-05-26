using System.Collections;
using UnityEngine;

public class FreeModeGameManager : GameManager
{
    [SerializeField] private float _resetCarDelay;
    [SerializeField] private float _enableInputDelay;

    private Vector3 _revivePosition;
    private Quaternion _reviveRotation;

    protected override void HandleAwake()
    {
        _carStuck.OnCarStuck += HandleCarStuck;
        _revivePosition = _car.transform.position;
        _reviveRotation = _car.transform.rotation;
    }

    protected override void HandleDestroy()
    {
        _gameActive = false;
        RaiseOnGameStopped();
    }

    protected override void HandleTransitionComplete()
    {
        StartCoroutine(EnableInputAfterDelay(_enableInputDelay));
        _gameActive = true;
        RaiseOnGameStarted();
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
}
