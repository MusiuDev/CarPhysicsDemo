using System;
using System.Collections;
using UnityEngine;

public class InfiniteDriftGameManager : MonoBehaviour
{
    public delegate void GameManagerEvent();
    public static event GameManagerEvent OnCarResetStarted;
    public static event GameManagerEvent OnCarResetCompleted;

    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private CarCollisionDetector _carCollision;
    [SerializeField] private CarStatsCotroller _carStats;
    [SerializeField] private CarStatusEffectScriptable _onCollisionStatusEffect;

    private Vector3 _revivePosition;
    private Quaternion _reviveRotation;
    private bool _resetting;

    void Awake()
    {
        CheckpointGroup.OnCheckpointGroupCleared += HandleCheckpointGroupCleared;
        _carCollision.OnCarCollisionWithObstacle += HandleCarCollision;
        _revivePosition = _car.transform.position;
        _reviveRotation = _car.transform.rotation;
    }

    private void HandleCarCollision(Collision col)
    {
        if (_resetting) return;

        _resetting = true;
        StartCoroutine(ResetCar());
    }

    private IEnumerator ResetCar()
    {
        OnCarResetStarted?.Invoke();
        yield return null;
        _carStats.ClearStatusEffects();
        _car.ResetToPositionAndRotation(_revivePosition, _reviveRotation);
        yield return null;
        OnCarResetCompleted?.Invoke();
        _carStats.AddStatusEffect(_onCollisionStatusEffect.statusEffect);
        _resetting = false;
    }

    private void HandleCheckpointGroupCleared(CheckpointGroup group)
    {
        if (group.ExitPoint)
        {
            _revivePosition = group.ExitPoint.position;
            _reviveRotation = group.ExitPoint.rotation;
        }
    }
}
