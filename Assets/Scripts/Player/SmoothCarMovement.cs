using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmoothCarMovement : MonoBehaviour
{
    [SerializeField] private CarStatsCotroller _statsController;
    [SerializeField] private float _centerOfMassVerticalOffset;
    [SerializeField] private CarWheel[] _wheels;
    [SerializeField] private float _wheelRaycastDistance;
    [SerializeField] private float _minMotionThreshold = 0.1f;
    [SerializeField] private float _fullMotionThreshold = 1f;

    private Rigidbody _rb;

    private CarState _state = new CarState();
    public ICarState State => _state;

    public IReadOnlyCollection<CarWheel> Wheels => _wheels;

    private CarMotionStats _movement => _statsController.Movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = Vector3.up * _centerOfMassVerticalOffset;
        _statsController.Initialize(_state);
        if (_rb.useGravity)
        {
            //We do not enforce it here to avoid the behavior of gravity being silently disabled on a rigidbody
            Debug.LogWarning("Smooth Car Movement Rigidbody has gravity enabled. This could lead to unexpected behavior. Consider disabling it");
        }
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
        _rb.position = position;
        _rb.rotation = rotation;
        _state.ResetMotion();
    }

    void FixedUpdate()
    {
        _statsController.UpdateMotionStats();
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
        _state.CurrentSpeedFactor = _rb.linearVelocity.magnitude / _movement.MaxSpeed;

        foreach (var wheel in _wheels)
        {
            wheel.UpdateContact(_wheelRaycastDistance);
            if (wheel.InContact)
            {
                _state.ContactingWheels++;
                _state.TotalSlopeTraction += _statsController.TractionBySlope.Evaluate(wheel.ContactSlope);
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
        _state.Speed = _rb.linearVelocity.magnitude;

        _state.DriftingFactor = 0f;
        _state.MotionAngle = 0;

        if (_state.Speed > _minMotionThreshold)
        {
            float angle = Vector3.Angle(_state.RbForward, _rb.linearVelocity.normalized);
            float motion = _state.Speed.Remap(_minMotionThreshold, _fullMotionThreshold, 0, 1);
            _state.DriftingFactor = (angle / 90f) * motion;
            _state.MotionAngle = (-(angle / 90f) + 1) * motion;
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
            float steerMult = Mathf.Max(_state.ContactingMult, _movement.AirControl) * _statsController.SteerBySpeed.Evaluate(_state.CurrentSpeedFactor);
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

        if (_movement.GravityScale > 0)
        {
            _rb.AddForce(Physics.gravity * _movement.GravityScale, ForceMode.Acceleration);
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
}
