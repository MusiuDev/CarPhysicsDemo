using System;
using System.Collections;
using UnityEngine;

public class InfiniteDriftGameManager : MonoBehaviour
{
    public delegate void GameManagerEvent();
    public static event GameManagerEvent OnCarResetStarted;
    public static event GameManagerEvent OnCarResetCompleted;
    public static event GameManagerEvent OnCarPreTeleport;
    public static event GameManagerEvent OnCarPostTeleport;

    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private CarCollisionDetector _carCollision;
    [SerializeField] private CarStuckDetection _carFlip;
    [SerializeField] private CarStatsCotroller _carStats;
    [SerializeField] private CarStatusEffectScriptable _onCollisionStatusEffect;
    [SerializeField] private CarStatusEffectScriptable _onResetStatusEffect;

    private Vector3 _revivePosition;
    private Quaternion _reviveRotation;
    private bool _resetting;

    void Awake()
    {
        CheckpointGroup.OnCheckpointGroupCleared += HandleCheckpointGroupCleared;
        _carCollision.OnCarCollisionWithObstacle += HandleCarCollision;
        _carFlip.OnCarStuck += HandleCarStuck;
        _revivePosition = _car.transform.position;
        _reviveRotation = _car.transform.rotation;
    }

    private void HandleCarCollision(Collision col)
    {
        if (_resetting) return;
        _resetting = true;
        StartCoroutine(ResetCar());
    }

    private void HandleCarStuck()
    {
        if (_resetting) return;
        _resetting = true;
        StartCoroutine(ResetCar());
    }

    private IEnumerator ResetCar()
    {
        OnCarResetStarted?.Invoke();
        _carStats.AddStatusEffect(_onCollisionStatusEffect);
        yield return new WaitForSeconds(1f);
        OnCarPreTeleport?.Invoke();
        yield return null;
        _carStats.ClearStatusEffects();
        _car.ResetToPositionAndRotation(_revivePosition, _reviveRotation);
        yield return null;
        OnCarPostTeleport?.Invoke();
        OnCarResetCompleted?.Invoke();
        _carStats.AddStatusEffect(_onResetStatusEffect);
        _resetting = false;
    }

    private void HandleCheckpointGroupCleared(CheckpointGroup group)
    {
        _revivePosition = group.ExitPosition;
        _reviveRotation = group.ExitRotation;
    }
}
