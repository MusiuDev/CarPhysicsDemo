using UnityEngine;

[CreateAssetMenu(fileName = "CarMovementCharacteristics", menuName = "Musiu Scriptables/CarMovementCharacteristics")]
public class CarMovementCharacteristics : ScriptableObject
{
    [field: SerializeField] public float _maxSpeed { get; private set; }
    [field: SerializeField] public float _acceleration { get; private set; }
    [field: SerializeField] public float _steerTorque { get; private set; }
    [field: SerializeField] public float _maxAngularSpeed { get; private set; }
    [field: SerializeField] public float _driftRecoverySpeed { get; private set; }
    [field: SerializeField] public float _fastDriftRecoveryThreshold { get; private set; }
    [field: SerializeField] public float _fastDriftRecoveryMult { get; private set; }
    [field: SerializeField] public AnimationCurve _steerMultCurveBySpeed { get; private set; }
    [field: SerializeField] public AnimationCurve _tractionCurveBySlope { get; private set; }
    [field: SerializeField] public float _tractionMutliplier { get; private set; }
    [field: SerializeField] public float _airControl { get; private set; }
    [field: SerializeField] public float _baseDamping { get; private set; }
    [field: SerializeField] public float _stoppedDamping { get; private set; }
    [field: SerializeField] public float _driftingDamping { get; private set; }
    [field: SerializeField] public float _baseAngularDamping { get; private set; }
    [field: SerializeField] public float _steeringAngularDamping { get; private set; }
    [field: SerializeField] public float _straightAngularDamping { get; private set; }
}