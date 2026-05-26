using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputController : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private InputActionReference _steerActionRef;
    [SerializeField] private InputActionReference _accelerateActionRef;
    [SerializeField] private InputActionReference _brakeActionRef;
    [SerializeField] private bool _autoAccelerate;
    [SerializeField] private bool _inputEnabledAtStart;

    private InputAction _steerAction;
    private InputAction _accelerateAction;
    private InputAction _brakeAction;

    private bool _inputEnabled;
    public bool InputEnabled
    {
        get
        {
            return _inputEnabled;
        }
        set
        {
            if (value == _inputEnabled) return;
            _inputEnabled = value;
            ResetInputState();
        }
    }
    public bool AutoAccelerate
    {
        get => _autoAccelerate;
        set => _autoAccelerate = value;
    }

    void Start()
    {
        if (!_car || !_steerActionRef || _steerActionRef.action == null || !_accelerateActionRef || !_brakeActionRef)
        {
            this.enabled = false;
            Debug.LogError("CarInputController is missing references. Disabling the component.");
            return;
        }

        _steerAction = _steerActionRef.action;
        _accelerateAction = _accelerateActionRef.action;
        _brakeAction = _brakeActionRef.action;

        if (_inputEnabledAtStart)
        {
            InputEnabled = true;
        }
    }

    void Update()
    {
        if (!InputEnabled) return;

        float steer = _steerAction.ReadValue<float>();
        bool accelerate = AutoAccelerate || (_accelerateAction.ReadValue<float>() > 0.5f);
        bool brake = _brakeAction.ReadValue<float>() > 0.5f;

        _car.SetInput(steer, accelerate, brake);
    }

    private void ResetInputState()
    {
        _car.SetInput(0f, false, false);
    }
}