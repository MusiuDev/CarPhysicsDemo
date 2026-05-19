using UnityEngine;

[CreateAssetMenu(fileName = "CarMovementCharacteristics", menuName = "Musiu Scriptables/CarMovementCharacteristics")]
public class CarMovementCharacteristics : ScriptableObject
{
    [field: SerializeField] public float MaxSpeed { get; private set; }
    [field: SerializeField] public float Acceleration { get; private set; }
    [field: SerializeField] public float SteerTorque { get; private set; }
    [field: SerializeField] public float MaxAngularSpeed { get; private set; }
    [field: SerializeField] public float DriftRecoverySpeed { get; private set; }
    [field: SerializeField] public float FastDriftRecoveryThreshold { get; private set; }
    [field: SerializeField] public float FastDriftRecoveryMult { get; private set; }
    [field: SerializeField] public AnimationCurve SteerMultCurveBySpeed { get; private set; }
    [field: SerializeField] public AnimationCurve TractionCurveBySlope { get; private set; }
    [field: SerializeField] public float TractionMutliplier { get; private set; }
    [field: SerializeField] public float AirControl { get; private set; }
    [field: SerializeField] public float BaseDamping { get; private set; }
    [field: SerializeField] public float StoppedDamping { get; private set; }
    [field: SerializeField] public float DriftingDamping { get; private set; }
    [field: SerializeField] public float BaseAngularDamping { get; private set; }
    [field: SerializeField] public float SteeringAngularDamping { get; private set; }
    [field: SerializeField] public float StraightAngularDamping { get; private set; }
}