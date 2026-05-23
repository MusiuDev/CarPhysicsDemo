using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmoothCarMovement : MonoBehaviour
{
    [SerializeField] private CarMovementCharacteristics _movementStraight;
    [SerializeField] private CarMovementCharacteristics _movementSteering;

    [SerializeField] private float _steeringTransitionTime;
    [SerializeField] private AnimationCurve _steeringTransitionCurve;

    [SerializeField] private float _centerOfMassVerticalOffset;
    [SerializeField] private CarWheel[] _wheels;
    [SerializeField] private float _wheelRaycastDistance;

    private Rigidbody _rb;

    private CarState _state = new CarState();
    public ICarState State => _state;

    public IReadOnlyCollection<CarWheel> Wheels => _wheels;
    private float _steeringTransitionState = 0f;
    private CarMovementCharacteristics _movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = Vector3.up * _centerOfMassVerticalOffset;
        _movement = Instantiate(_movementStraight);
    }

    public void SetInput(float steer, bool accelerate, bool brake)
    {
        _state.SteerInput = steer;
        _state.AccelerateInput = accelerate;
        _state.BrakeInput = brake;
    }

    public void ResetToPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = position;
        transform.rotation = rotation;
        _state.ResetMotion();
    }

    void FixedUpdate()
    {
        UpdateWheelsInfo();
        UpdateMotionInfo();
        UpdateMovementCharacteristicsFromSteering(); //TODO: Rename this
        UpdateSteering();
        UpdateMotion();
        UpdateDamping();
    }

    private void UpdateWheelsInfo()
    {
        _state.ContactingWheels = 0;
        _state.TotalSlopeTraction = 0f;
        _state.CurrentSpeedFactor = _rb.linearVelocity.magnitude / _movement.MaxSpeed;

        foreach (var wheel in _wheels)
        {
            wheel.UpdateContact(_wheelRaycastDistance);
            if (wheel.InContact)
            {
                _state.ContactingWheels++;
                _state.TotalSlopeTraction += _movement.TractionCurveBySlope.Evaluate(wheel.ContactSlope);
            }
        }
        float totalContact = (1f / _wheels.Length) * _state.ContactingWheels;
        _state.TotalSlopeTraction /= _wheels.Length;

        _state.ContactingMult = totalContact * _state.TotalSlopeTraction * _movement.TractionMutliplier;
    }

    private void UpdateMotionInfo()
    {
        _state.RbForward = _rb.rotation * Vector3.forward;
        _state.RbUp = _rb.rotation * Vector3.up;

        _state.DriftingFactor = 0f;
        _state.MotionAngle = 0;

        if (_rb.linearVelocity.magnitude > 0.01f)
        {
            float angle = Vector3.Angle(_state.RbForward, _rb.linearVelocity.normalized);
            _state.DriftingFactor = angle / 90f;
            _state.MotionAngle = -(angle / 90f) + 1;
        }
    }

    private void UpdateDamping()
    {
        float linearDamping = _movement.BaseDamping;

        if (!_state.AccelerateInput && !_state.BrakeInput)
        {
            linearDamping += _movement.StoppedDamping * _state.ContactingMult;
        }

        linearDamping += _state.DriftingFactor * _movement.DriftingDamping * _state.ContactingMult;

        _rb.linearDamping = linearDamping;


        float angularDamping = _movement.BaseAngularDamping;
        if (Mathf.Abs(_state.SteerInput) > 0.1f)
        {
            angularDamping += _movement.SteeringAngularDamping * _state.ContactingMult;
        }
        else
        {
            angularDamping += _movement.StraightAngularDamping * _state.ContactingMult;
        }

        _rb.angularDamping = angularDamping;
    }

    private void UpdateSteering()
    {
        if (Mathf.Abs(_state.SteerInput) <= 0.1f && (_state.DriftingFactor < 0.1f) || _state.MotionAngle < -0.1f)
        {
            _state.IntentionAngle = 0;
        }

        if (!_state.AccelerateInput && _state.BrakeInput)
        {
            if (_state.IntentionAngle == 0f)
            {
                _state.IntentionAngle = -1;
            }
        }
        else
        {
            _state.IntentionAngle = 1;
        }

        if (Mathf.Abs(_state.SteerInput) > 0.1f)
        {
            float steerMult = Mathf.Max(_state.ContactingMult, _movement.AirControl) * _movement.SteerMultCurveBySpeed.Evaluate(_state.CurrentSpeedFactor);
            if (_state.ContactingWheels == 0)
            {
                steerMult = Mathf.Max(steerMult, _movement.AirControl);
            }

            _rb.AddRelativeTorque(0, _movement.SteerTorque * _state.SteerInput * steerMult * Time.fixedDeltaTime * _state.IntentionAngle, 0, ForceMode.Acceleration);

            Vector3 currentAngularVelocity = _rb.angularVelocity;
            float localYAngularVelocity = Vector3.Dot(currentAngularVelocity, _state.RbUp) * Mathf.Rad2Deg;
            if (Mathf.Abs(localYAngularVelocity) > _movement.MaxAngularSpeed)
            {
                float excessYSpin = localYAngularVelocity - Mathf.Clamp(localYAngularVelocity, -_movement.MaxAngularSpeed, _movement.MaxAngularSpeed);
                Vector3 reduction = _state.RbUp * excessYSpin * Mathf.Deg2Rad;
                _rb.angularVelocity = Vector3.Lerp(currentAngularVelocity, currentAngularVelocity - reduction, steerMult);
            }
        }
    }

    private void UpdateMovementCharacteristicsFromSteering()
    {
        if (Mathf.Abs(_state.SteerInput) > 0.1f)
        {
            float steerMult = _movement.SteerMultCurveBySpeed.Evaluate(_state.CurrentSpeedFactor);
            _steeringTransitionState += (Time.deltaTime / _steeringTransitionTime) * steerMult * Mathf.Abs(_state.SteerInput);
        }
        else
        {
            _steeringTransitionState -= Time.deltaTime / _steeringTransitionTime;
        }

        _steeringTransitionState = Mathf.Clamp01(_steeringTransitionState);
        float curvedTransition = _steeringTransitionCurve.Evaluate(_steeringTransitionState);

        var objectType = typeof(CarMovementCharacteristics);
        var props = objectType.GetProperties();
        foreach (var prop in props)
        {
            object value = prop.GetValue(_movement, null);
            if (value != null && value is float)
            {
                float valueStraight = (float)prop.GetValue(_movementStraight, null);
                float valueSteering = (float)prop.GetValue(_movementSteering, null);
                float lerped = Mathf.Lerp(valueStraight, valueSteering, curvedTransition);
                prop.SetValue(_movement, lerped);
            }
        }
    }

    private void UpdateMotion()
    {
        if (_state.AccelerateInput)
        {
            _rb.AddForce(_state.RbForward * _state.ContactingMult * _movement.Acceleration, ForceMode.Force);
        }
        if (_state.BrakeInput)
        {
            _rb.AddForce(-_state.RbForward * _state.ContactingMult * _movement.Acceleration, ForceMode.Force);
        }

        if (_movement.DriftRecoverySpeed > 0)
        {
            float recoverySpeed = Mathf.Deg2Rad * _movement.DriftRecoverySpeed * Time.fixedDeltaTime;
            if (_state.DriftingFactor > _movement.FastDriftRecoveryThreshold)
            {
                float m = Mathf.Lerp(1, _movement.FastDriftRecoveryMult, (_state.DriftingFactor - _movement.FastDriftRecoveryThreshold) / (1 - _movement.FastDriftRecoveryThreshold));
                recoverySpeed *= m;
            }

            Vector3 targetDirection = Vector3.MoveTowards(_rb.linearVelocity.normalized, _state.IntentionAngle < 0 ? -_state.RbForward : _state.RbForward, recoverySpeed).normalized;
            float velocityMagnitude = _rb.linearVelocity.magnitude;

            if (velocityMagnitude > _movement.MaxSpeed)
            {
                velocityMagnitude = Mathf.Lerp(velocityMagnitude, _movement.MaxSpeed, _state.ContactingMult);
            }
            Vector3 targetVelocity = targetDirection * velocityMagnitude;

            _rb.linearVelocity = Vector3.Slerp(_rb.linearVelocity, targetVelocity, _state.ContactingMult);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach (var wheel in _wheels)
        {
            Ray ray = wheel.GetRay();
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * _wheelRaycastDistance);
        }
    }

    public class CarState : ICarState
    {
        public float SteerInput { get; set; }
        public bool AccelerateInput { get; set; }
        public bool BrakeInput { get; set; }
        public float ContactingMult { get; set; }
        public int ContactingWheels { get; set; }
        public float TotalSlopeTraction { get; set; }
        public float DriftingFactor { get; set; }
        public int IntentionAngle { get; set; }
        public float CurrentSpeedFactor { get; set; }
        public Vector3 RbForward { get; set; }
        public Vector3 RbUp { get; set; }
        public float MotionAngle { get; set; }

        public void ResetMotion()
        {
            SteerInput = default;
            AccelerateInput = default;
            BrakeInput = default;
            ContactingMult = default;
            ContactingWheels = default;
            TotalSlopeTraction = default;
            DriftingFactor = default;
            IntentionAngle = default;
            CurrentSpeedFactor = default;
            RbForward = default;
            RbUp = default;
            MotionAngle = default;
        }
    }

    public interface ICarState
    {
        float SteerInput { get; }
        bool AccelerateInput { get; }
        bool BrakeInput { get; }
        float ContactingMult { get; }
        int ContactingWheels { get; }
        float TotalSlopeTraction { get; }
        float DriftingFactor { get; }
        int IntentionAngle { get; }
        float CurrentSpeedFactor { get; }
        Vector3 RbForward { get; }
        Vector3 RbUp { get; }
        float MotionAngle { get; }
    }
}
