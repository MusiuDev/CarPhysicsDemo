using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmoothCarMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _acceleration;

    [SerializeField] private float _steerTorque;
    [SerializeField] private AnimationCurve _steerMultCurveBySpeed;
    [SerializeField] private float _maxAngularSpeed;
    [SerializeField] private float _driftRecoverySpeed;
    [SerializeField] private float _fastDriftRecoveryThreshold = 1f;
    [SerializeField] private float _fastDriftRecoveryMult = 3;
    [SerializeField] private AnimationCurve _tractionCurveBySlope; //1 = flat, 0 = vertical or worse.
    [SerializeField] private float _tractionMutliplier = 1f;
    [SerializeField] private float _airControl = 0.1f;

    [SerializeField] private float _baseDamping;
    [SerializeField] private float _stoppedDamping;
    [SerializeField] private float _driftingDamping;

    [SerializeField] private float _baseAngularDamping;
    [SerializeField] private float _steeringAngularDamping;
    [SerializeField] private float _straightAngularDamping;

    [SerializeField] private float _centerOfMassVerticalOffset;

    [SerializeField] private Transform[] _frontWheels;
    [SerializeField] private Transform[] _backWheels;

    [SerializeField] private float _raycastDistance;

    Rigidbody _rb;

    private float _steerInput;
    public float SteerInput => _steerInput;
    private bool _accelerateInput;
    public bool AccelerateInput => _accelerateInput;
    private bool _brakeInput;
    public bool BrakeInput => _brakeInput;

    private float _contactingMult;
    private int _contactingWheels;
    private float _totalTraction;
    private float _driftingFactor;
    private int _intentionAngle;
    private float _currentSpeedFactor;

    public float DriftingFactor => _driftingFactor;

    private Dictionary<Transform, bool> _contactingDict = new Dictionary<Transform, bool>();
    public IReadOnlyDictionary<Transform, bool> ContacticDict => _contactingDict;

    private Dictionary<Transform, float> _slopeDict = new Dictionary<Transform, float>();

    private List<Transform> _allWheels;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = Vector3.up * _centerOfMassVerticalOffset;

        _allWheels = new List<Transform>();

        for (int i = 0; i < _frontWheels.Length; i++)
        {
            _allWheels.Add(_frontWheels[i]);
            _contactingDict.Add(_frontWheels[i], false);
            _slopeDict.Add(_frontWheels[i], -1f);
        }

        for (int i = 0; i < _backWheels.Length; i++)
        {
            _allWheels.Add(_backWheels[i]);
            _contactingDict.Add(_backWheels[i], false);
            _slopeDict.Add(_backWheels[i], -1f);
        }
    }

    public void SetInput(float steer, bool accelerate, bool brake)
    {
        _steerInput = steer;
        _accelerateInput = accelerate;
        _brakeInput = brake;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        _contactingWheels = 0;
        _totalTraction = 0f;
        _currentSpeedFactor = _rb.linearVelocity.magnitude / _maxSpeed;

        for (int i = 0; i < _allWheels.Count; i++)
        {
            Transform wheel = _allWheels[i];
            Ray ray = new Ray(wheel.position, -transform.up);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, _raycastDistance))
            {
                _contactingWheels++;
                _contactingDict[wheel] = true;
                float slope = Mathf.Clamp01(Vector3.Dot(hitInfo.normal, Vector3.up));
                _slopeDict[wheel] = slope;
                _totalTraction += slope / _allWheels.Count;
            }
            else
            {
                _contactingDict[wheel] = false;
                _slopeDict[wheel] = -1f;
            }
        }

        Vector3 rbForward = _rb.rotation * Vector3.forward;
        Vector3 rbUp = _rb.rotation * Vector3.up;

        _contactingMult = (1f / _allWheels.Count) * _contactingWheels * _tractionCurveBySlope.Evaluate(_totalTraction) * _tractionMutliplier;

        _driftingFactor = 0f;
        float motionAngle = 0;

        if (_rb.linearVelocity.magnitude > 0.1f)
        {
            float dot = Vector3.Dot(rbForward, _rb.linearVelocity.normalized);
            _driftingFactor = 1f - Mathf.Abs(dot);
            motionAngle = dot;
        }


        if (_accelerateInput)
        {
            _rb.AddForce(rbForward * _contactingMult * _acceleration, ForceMode.Force);
        }
        if (_brakeInput)
        {
            _rb.AddForce(-rbForward * _contactingMult * _acceleration, ForceMode.Force);
        }

        if (Mathf.Abs(_steerInput) <= 0.1f && (_driftingFactor < 0.1f) || motionAngle < -0.1f)
        {
            _intentionAngle = 0;
        }

        if (!_accelerateInput && _brakeInput)
        {
            if (_intentionAngle == 0f)
            {
                _intentionAngle = -1;
            }
        }
        else
        {
            _intentionAngle = 1;
        }

        if (Mathf.Abs(_steerInput) > 0.1f)
        {
            float steerMult = _contactingMult * _steerMultCurveBySpeed.Evaluate(_currentSpeedFactor);
            if (_contactingWheels == 0)
            {
                steerMult = _airControl;
            }

            _rb.AddRelativeTorque(0, _steerTorque * _steerInput * steerMult * Time.fixedDeltaTime * _intentionAngle, 0, ForceMode.Acceleration);

            Vector3 currentAngularVelocity = _rb.angularVelocity;
            float localYAngularVelocity = Vector3.Dot(currentAngularVelocity, rbUp) * Mathf.Rad2Deg;
            if (Mathf.Abs(localYAngularVelocity) > _maxAngularSpeed)
            {
                float excessYSpin = localYAngularVelocity - Mathf.Clamp(localYAngularVelocity, -_maxAngularSpeed, _maxAngularSpeed);
                Vector3 reduction = rbUp * excessYSpin * Mathf.Deg2Rad;
                _rb.angularVelocity = Vector3.Lerp(currentAngularVelocity, currentAngularVelocity - reduction, steerMult);
            }
        }


        if (_driftRecoverySpeed > 0)
        {
            float recoverySpeed = Mathf.Deg2Rad * _driftRecoverySpeed * Time.fixedDeltaTime;
            if (_driftingFactor > _fastDriftRecoveryThreshold)
            {
                float m = Mathf.Lerp(1, _fastDriftRecoveryMult, (_driftingFactor - _fastDriftRecoveryThreshold) / (1 - _fastDriftRecoveryThreshold));
                recoverySpeed *= m;
            }

            Vector3 targetDirection = Vector3.MoveTowards(_rb.linearVelocity.normalized, _intentionAngle < 0 ? -rbForward : rbForward, recoverySpeed).normalized;
            float velocityMagnitude = _rb.linearVelocity.magnitude;

            if (velocityMagnitude > _maxSpeed)
            {
                velocityMagnitude = Mathf.Lerp(velocityMagnitude, _maxSpeed, _contactingMult);
            }
            Vector3 targetVelocity = targetDirection * velocityMagnitude;

            _rb.linearVelocity = Vector3.Slerp(_rb.linearVelocity, targetVelocity, _contactingMult);
        }


        float linearDamping = _baseDamping;

        if (!_accelerateInput && !_brakeInput)
        {
            linearDamping += _stoppedDamping * _contactingMult;
        }

        linearDamping += _driftingFactor * _driftingDamping * _contactingMult;

        _rb.linearDamping = linearDamping;


        float angularDamping = _baseAngularDamping;
        if (Mathf.Abs(_steerInput) > 0.1f)
        {
            angularDamping += _steeringAngularDamping * _contactingMult;
        }
        else
        {
            angularDamping += _straightAngularDamping * _contactingMult;
        }

        _rb.angularDamping = angularDamping;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < _frontWheels.Length; i++)
        {
            Transform wheel = _frontWheels[i];
            Gizmos.DrawLine(wheel.transform.position, wheel.transform.position - transform.up * _raycastDistance);
        }


        for (int i = 0; i < _backWheels.Length; i++)
        {
            Transform wheel = _backWheels[i];
            Gizmos.DrawLine(wheel.transform.position, wheel.transform.position - transform.up * _raycastDistance);
        }
    }
}
