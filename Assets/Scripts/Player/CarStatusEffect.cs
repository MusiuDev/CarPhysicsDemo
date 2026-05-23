using UnityEngine;

[System.Serializable]
public class CarStatusEffect
{
    public AnimationCurve effectCurve;
    public float effectDuration;
    public CarStatOverride[] effectValues;

    public void ApplyTo(CarMotionStats target, float t)
    {
        if (!target) return;
        if (effectDuration == 0) return;
        if (effectValues == null || effectValues.Length == 0) return;

        float normalizedT = Mathf.Clamp01(t / effectDuration);
        float currentStrength = effectCurve.Evaluate(normalizedT);
        foreach (var item in effectValues)
        {
            var (get, set) = CarStatsRegistry.Fields[item.type];
            float currentValue = get(target);
            float modifiedValue = currentValue;

            if (item.style == StatOverrideOperation.Set)
            {
                modifiedValue = Mathf.Lerp(currentValue, item.value, currentStrength);
            }
            else if (item.style == StatOverrideOperation.Mult)
            {
                float currentMult = Mathf.Lerp(1, item.value, currentStrength);
                modifiedValue = currentValue * currentMult;
            }

            set(target, modifiedValue);
        }
    }
}