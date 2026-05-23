using UnityEngine;

[System.Serializable]
public class CarStatusEffect
{
    public AnimationCurve effectCurve;
    public float effectDuration;

    public float steerTorque = 1f;
    public float maxAngularSpeed = 1f;
    public float traction = 1f;
    public float driftRecovery = 1f;
    public float fastDriftRecoveryThreshold = 1f;
    public float fastDriftRecovery = 1f;
    public float acceleration = 1f;
    public float maxSpeed = 1f;
    public float airControl = 1f;
}
