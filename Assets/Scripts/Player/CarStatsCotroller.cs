using System.Collections.Generic;
using UnityEngine;

public class CarStatsCotroller : MonoBehaviour
{
    [SerializeField] private CarMotionStats _movementStraight;
    [SerializeField] private CarMotionStats _movementSteering;

    [SerializeField] private AnimationCurve _steerMultCurveBySpeed;
    [SerializeField] private AnimationCurve _tractionCurveBySlope;
    [SerializeField] private AnimationCurve _steeringTransitionCurve;

    [SerializeField] private float _steeringTransitionTime;

    public AnimationCurve SteerBySpeed => _steerMultCurveBySpeed;
    public AnimationCurve TractionBySlope => _tractionCurveBySlope;

    private CarMotionStats _movement;
    public CarMotionStats Movement => _movement;

    private ICarState _state;
    private float _steeringTransitionState = 0f;
    private Dictionary<CarStatusEffect, float> _effectTimers = new Dictionary<CarStatusEffect, float>();
    //uses a list too to ensure effects are applied in the same order they were added in the intra frame loop.
    private List<CarStatusEffect> _activeEffects = new List<CarStatusEffect>();

    public void Initialize(ICarState carState)
    {
        _movement = Instantiate(_movementStraight);
        _state = carState;
    }

    public void UpdateMotionStats()
    {
        if (Mathf.Abs(_state.SteerInput) > 0.1f)
        {
            float steerMult = _steerMultCurveBySpeed.Evaluate(_state.CurrentSpeedFactor);
            _steeringTransitionState += (Time.deltaTime / _steeringTransitionTime) * steerMult * Mathf.Abs(_state.SteerInput);
        }
        else
        {
            _steeringTransitionState -= Time.deltaTime / _steeringTransitionTime;
        }

        _steeringTransitionState = Mathf.Clamp01(_steeringTransitionState);
        float curvedTransition = _steeringTransitionCurve.Evaluate(_steeringTransitionState);

        _movement.MaxSpeed = Mathf.Lerp(_movementStraight.MaxSpeed, _movementSteering.MaxSpeed, curvedTransition);
        _movement.Acceleration = Mathf.Lerp(_movementStraight.Acceleration, _movementSteering.Acceleration, curvedTransition);
        _movement.SteerTorque = Mathf.Lerp(_movementStraight.SteerTorque, _movementSteering.SteerTorque, curvedTransition);
        _movement.MaxAngularSpeed = Mathf.Lerp(_movementStraight.MaxAngularSpeed, _movementSteering.MaxAngularSpeed, curvedTransition);
        _movement.DriftRecoverySpeed = Mathf.Lerp(_movementStraight.DriftRecoverySpeed, _movementSteering.DriftRecoverySpeed, curvedTransition);
        _movement.FastDriftRecoveryThreshold = Mathf.Lerp(_movementStraight.FastDriftRecoveryThreshold, _movementSteering.FastDriftRecoveryThreshold, curvedTransition);
        _movement.FastDriftRecoveryMult = Mathf.Lerp(_movementStraight.FastDriftRecoveryMult, _movementSteering.FastDriftRecoveryMult, curvedTransition);
        _movement.TractionMutliplier = Mathf.Lerp(_movementStraight.TractionMutliplier, _movementSteering.TractionMutliplier, curvedTransition);
        _movement.AirControl = Mathf.Lerp(_movementStraight.AirControl, _movementSteering.AirControl, curvedTransition);
        _movement.BaseDamping = Mathf.Lerp(_movementStraight.BaseDamping, _movementSteering.BaseDamping, curvedTransition);
        _movement.StoppedDamping = Mathf.Lerp(_movementStraight.StoppedDamping, _movementSteering.StoppedDamping, curvedTransition);
        _movement.DriftingDamping = Mathf.Lerp(_movementStraight.DriftingDamping, _movementSteering.DriftingDamping, curvedTransition);
        _movement.BaseAngularDamping = Mathf.Lerp(_movementStraight.BaseAngularDamping, _movementSteering.BaseAngularDamping, curvedTransition);
        _movement.SteeringAngularDamping = Mathf.Lerp(_movementStraight.SteeringAngularDamping, _movementSteering.SteeringAngularDamping, curvedTransition);
        _movement.StraightAngularDamping = Mathf.Lerp(_movementStraight.StraightAngularDamping, _movementSteering.StraightAngularDamping, curvedTransition);

        ApplyActiveEffects();
    }

    public void AddStatusEffect(CarStatusEffect effect)
    {
        if (effect == null || effect.effectDuration <= 0.01f)
        {
            Debug.LogWarning("Tried to add a zero-duration status effect. Ignoring...");
            return;
        }

        _activeEffects.Add(effect);
        _effectTimers.Add(effect, 0);
    }

    public void ApplyActiveEffects()
    {
        foreach (var effect in _activeEffects)
        {
            _effectTimers[effect] += Time.deltaTime;
            float effectTimerNormalized = Mathf.Clamp01(_effectTimers[effect] / effect.effectDuration);

            float effectStrength = effect.effectCurve.Evaluate(effectTimerNormalized);

            _movement.SteerTorque *= Mathf.Lerp(1, effect.steerTorque, effectStrength);
            _movement.MaxAngularSpeed *= Mathf.Lerp(1, effect.maxAngularSpeed, effectStrength);
            _movement.TractionMutliplier *= Mathf.Lerp(1, effect.traction, effectStrength);
            _movement.DriftRecoverySpeed *= Mathf.Lerp(1, effect.driftRecovery, effectStrength);
            _movement.FastDriftRecoveryThreshold *= Mathf.Lerp(1, effect.fastDriftRecoveryThreshold, effectStrength);
            _movement.FastDriftRecoveryMult *= Mathf.Lerp(1, effect.fastDriftRecovery, effectStrength);
            _movement.Acceleration *= Mathf.Lerp(1, effect.acceleration, effectStrength);
            _movement.MaxSpeed *= Mathf.Lerp(1, effect.maxSpeed, effectStrength);
            _movement.AirControl *= Mathf.Lerp(1, effect.airControl, effectStrength);
        }
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            if (_effectTimers[effect] >= effect.effectDuration)
            {
                RemoveStatusEffect(effect);
            }
        }
    }

    private void RemoveStatusEffect(CarStatusEffect effect)
    {
        _activeEffects.Remove(effect);
        _effectTimers.Remove(effect);
    }

    public void ClearStatusEffects()
    {
        _activeEffects.Clear();
        _effectTimers.Clear();
    }
}
