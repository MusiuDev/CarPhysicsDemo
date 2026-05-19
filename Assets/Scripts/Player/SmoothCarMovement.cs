using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmoothCarMovement : MonoBehaviour
{
    [SerializeField] private CarMovementCharacteristics _movement;
    [SerializeField] private float _centerOfMassVerticalOffset;
    [SerializeField] private CarWheel[] _wheels;
    [SerializeField] private float _wheelRaycastDistance;

    private Rigidbody _rb;

    private CarState _state = new CarState();
    public ICarState State => _state;

    public IReadOnlyCollection<CarWheel> Wheels => _wheels;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = Vector3.up * _centerOfMassVerticalOffset;
    }

    public void SetInput(float steer, bool accelerate, bool brake)
    {
        _state.SteerInput = steer;
        _state.AccelerateInput = accelerate;
        _state.BrakeInput = brake;
    }

    void FixedUpdate()
    {
        UpdateWheelsInfo();
        UpdateMotionInfo();
        UpdateSteering();
        UpdateMotion();
        UpdateDamping();
    }

    private void UpdateWheelsInfo()
    {
        _state.ContactingWheels = 0;
        _state.TotalSlopeTraction = 0f;
        _state.CurrentSpeedFactor = _rb.linearVelocity.magnitude / _movement._maxSpeed;

        foreach (var wheel in _wheels)
        {
            wheel.UpdateContact(_wheelRaycastDistance);
            if (wheel.InContact)
            {
                _state.ContactingWheels++;
                _state.TotalSlopeTraction += wheel.ContactSlope;
            }
        }

        _state.ContactingMult = (1f / _wheels.Length) * _state.ContactingWheels * _movement._tractionCurveBySlope.Evaluate(_state.TotalSlopeTraction) * _movement._tractionMutliplier;
    }

    private void UpdateMotionInfo()
    {
        _state.RbForward = _rb.rotation * Vector3.forward;
        _state.RbUp = _rb.rotation * Vector3.up;

        _state.DriftingFactor = 0f;
        _state.MotionAngle = 0;

        if (_rb.linearVelocity.magnitude > 0.1f)
        {
            float dot = Vector3.Dot(_state.RbForward, _rb.linearVelocity.normalized);
            _state.DriftingFactor = 1f - Mathf.Abs(dot);
            _state.MotionAngle = dot;
        }
    }

    private void UpdateDamping()
    {
        float linearDamping = _movement._baseDamping;

        if (!_state.AccelerateInput && !_state.BrakeInput)
        {
            linearDamping += _movement._stoppedDamping * _state.ContactingMult;
        }

        linearDamping += _state.DriftingFactor * _movement._driftingDamping * _state.ContactingMult;

        _rb.linearDamping = linearDamping;


        float angularDamping = _movement._baseAngularDamping;
        if (Mathf.Abs(_state.SteerInput) > 0.1f)
        {
            angularDamping += _movement._steeringAngularDamping * _state.ContactingMult;
        }
        else
        {
            angularDamping += _movement._straightAngularDamping * _state.ContactingMult;
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
            float steerMult = _state.ContactingMult * _movement._steerMultCurveBySpeed.Evaluate(_state.CurrentSpeedFactor);
            if (_state.ContactingWheels == 0)
            {
                steerMult = _movement._airControl;
            }

            _rb.AddRelativeTorque(0, _movement._steerTorque * _state.SteerInput * steerMult * Time.fixedDeltaTime * _state.IntentionAngle, 0, ForceMode.Acceleration);

            Vector3 currentAngularVelocity = _rb.angularVelocity;
            float localYAngularVelocity = Vector3.Dot(currentAngularVelocity, _state.RbUp) * Mathf.Rad2Deg;
            if (Mathf.Abs(localYAngularVelocity) > _movement._maxAngularSpeed)
            {
                float excessYSpin = localYAngularVelocity - Mathf.Clamp(localYAngularVelocity, -_movement._maxAngularSpeed, _movement._maxAngularSpeed);
                Vector3 reduction = _state.RbUp * excessYSpin * Mathf.Deg2Rad;
                _rb.angularVelocity = Vector3.Lerp(currentAngularVelocity, currentAngularVelocity - reduction, steerMult);
            }
        }
    }

    private void UpdateMotion()
    {
        if (_state.AccelerateInput)
        {
            _rb.AddForce(_state.RbForward * _state.ContactingMult * _movement._acceleration, ForceMode.Force);
        }
        if (_state.BrakeInput)
        {
            _rb.AddForce(-_state.RbForward * _state.ContactingMult * _movement._acceleration, ForceMode.Force);
        }

        if (_movement._driftRecoverySpeed > 0)
        {
            float recoverySpeed = Mathf.Deg2Rad * _movement._driftRecoverySpeed * Time.fixedDeltaTime;
            if (_state.DriftingFactor > _movement._fastDriftRecoveryThreshold)
            {
                float m = Mathf.Lerp(1, _movement._fastDriftRecoveryMult, (_state.DriftingFactor - _movement._fastDriftRecoveryThreshold) / (1 - _movement._fastDriftRecoveryThreshold));
                recoverySpeed *= m;
            }

            Vector3 targetDirection = Vector3.MoveTowards(_rb.linearVelocity.normalized, _state.IntentionAngle < 0 ? -_state.RbForward : _state.RbForward, recoverySpeed).normalized;
            float velocityMagnitude = _rb.linearVelocity.magnitude;

            if (velocityMagnitude > _movement._maxSpeed)
            {
                velocityMagnitude = Mathf.Lerp(velocityMagnitude, _movement._maxSpeed, _state.ContactingMult);
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
