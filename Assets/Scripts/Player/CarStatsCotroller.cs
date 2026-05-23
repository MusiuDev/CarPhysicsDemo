using System.Collections.Generic;
using UnityEngine;

public class CarStatsCotroller : MonoBehaviour
{
    [SerializeField] private CarMotionStats _movementStraight;
    [SerializeField] private CarMotionStatsOverride _movementSteering;

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
        ResetAllValues();
        LerpValues(curvedTransition);
        ApplyActiveEffects();
    }

    public void LerpValues(float normalizedT)
    {
        if (!_movementStraight) return;
        if (!_movementSteering) return;

        if (_movementSteering.overrides == null || _movementSteering.overrides.Length == 0) return;

        foreach (var item in _movementSteering.overrides)
        {
            var (get, set) = CarStatsRegistry.Fields[item.type];
            float currentValue = get(_movementStraight);
            float modifiedValue = currentValue;

            if (item.style == StatOverrideOperation.Set)
            {
                modifiedValue = Mathf.Lerp(currentValue, item.value, normalizedT);
            }
            else if (item.style == StatOverrideOperation.Mult)
            {
                float currentMult = Mathf.Lerp(1, item.value, normalizedT);
                modifiedValue = currentValue * currentMult;
            }

            set(_movement, modifiedValue);
        }
    }

    private void ResetAllValues()
    {
        //TODO: cache both the enum array and the getters and setters;
        var values = (CarStatType[])System.Enum.GetValues(typeof(CarStatType));
        foreach (var item in values)
        {
            var (get, set) = CarStatsRegistry.Fields[item];
            float straightValue = get(_movementStraight);
            set(_movement, straightValue);
        }
    }

    public void AddStatusEffect(CarStatusEffectScriptable effect)
    {
        AddStatusEffect(effect.statusEffect);
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
            effect.ApplyTo(_movement, _effectTimers[effect]);
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
