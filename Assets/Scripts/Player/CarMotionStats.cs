using UnityEngine;

[CreateAssetMenu(fileName = "CarMotionStats", menuName = "Custom Scriptables/CarMotionStats")]
public class CarMotionStats : ScriptableObject
{
    public float MaxSpeed;
    public float Acceleration;
    public float SteerTorque;
    public float MaxAngularSpeed;
    public float DriftRecoverySpeed;
    public float FastDriftRecoveryThreshold;
    public float FastDriftRecoveryMult;
    public float TractionMutliplier;
    public float AirControl;
    public float BaseDamping;
    public float StoppedDamping;
    public float DriftingDamping;
    public float BaseAngularDamping;
    public float SteeringAngularDamping;
    public float StraightAngularDamping;
}
