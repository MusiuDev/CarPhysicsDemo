using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CarInputController : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private InputActionReference _steerActionRef;
    [SerializeField] private InputActionReference _accelerateActionRef;
    [SerializeField] private InputActionReference _brakeActionRef;
    [SerializeField] private bool _autoAccelerate;
    [SerializeField] private bool _inputEnabledAtStart;
    [SerializeField] private UIDocument _inputHUD;
    [SerializeField] private bool _forceTouchInputs;

    private InputAction _steerAction;
    private InputAction _accelerateAction;
    private InputAction _brakeAction;

    private VisualElement _steerLeftUIButton;
    private VisualElement _steerRightUIButton;
    private VisualElement _accelerateUIButton;
    private VisualElement _brakeUIButton;

    private float _uiSteer;
    private bool _uiAccel;
    private bool _uiBrake;

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
        SetupUIInputs();

    }

    private void SetupUIInputs()
    {
        if (!_inputHUD) return;
        VisualElement root = _inputHUD.rootVisualElement;

        bool hasTouch = Touchscreen.current != null;
        if (!hasTouch && !_forceTouchInputs)
        {
            VisualElement inputContainer = root.Query<VisualElement>("InputButtonsContainer");
            if (inputContainer != null) inputContainer.style.display = DisplayStyle.None;
            return;
        }

        _steerLeftUIButton = root.Query<VisualElement>("SteerLeft_Btn");
        _steerRightUIButton = root.Query<VisualElement>("SteerRight_Btn");
        _accelerateUIButton = root.Query<VisualElement>("Aceelerate_Btn");
        _brakeUIButton = root.Query<VisualElement>("Brake_Btn");

        if (_steerLeftUIButton != null)
        {
            _steerLeftUIButton.RegisterCallback<PointerDownEvent>(_ => _uiSteer = -1f, TrickleDown.TrickleDown);
            _steerLeftUIButton.RegisterCallback<PointerUpEvent>(_ => _uiSteer = 0);
            _steerLeftUIButton.RegisterCallback<PointerLeaveEvent>(_ => _uiSteer = 0);
            _steerLeftUIButton.RegisterCallback<PointerCancelEvent>(_ => _uiSteer = 0);
        }

        if (_steerRightUIButton != null)
        {
            _steerRightUIButton.RegisterCallback<PointerDownEvent>(_ => _uiSteer = 1f, TrickleDown.TrickleDown);
            _steerRightUIButton.RegisterCallback<PointerUpEvent>(_ => _uiSteer = 0);
            _steerRightUIButton.RegisterCallback<PointerLeaveEvent>(_ => _uiSteer = 0);
            _steerRightUIButton.RegisterCallback<PointerCancelEvent>(_ => _uiSteer = 0);
        }

        if (_accelerateUIButton != null)
        {
            _accelerateUIButton.RegisterCallback<PointerDownEvent>(_ => _uiAccel = true, TrickleDown.TrickleDown);
            _accelerateUIButton.RegisterCallback<PointerUpEvent>(_ => _uiAccel = false);
            _accelerateUIButton.RegisterCallback<PointerLeaveEvent>(_ => _uiAccel = false);
            _accelerateUIButton.RegisterCallback<PointerCancelEvent>(_ => _uiAccel = false);
        }

        if (_brakeUIButton != null)
        {
            _brakeUIButton.RegisterCallback<PointerDownEvent>(_ => _uiBrake = true, TrickleDown.TrickleDown);
            _brakeUIButton.RegisterCallback<PointerUpEvent>(_ => _uiBrake = false);
            _brakeUIButton.RegisterCallback<PointerLeaveEvent>(_ => _uiBrake = false);
            _brakeUIButton.RegisterCallback<PointerCancelEvent>(_ => _uiBrake = false);
        }
    }

    void Update()
    {
        if (!InputEnabled) return;

        float steer = Mathf.Clamp((_steerAction.ReadValue<float>() + _uiSteer), -1, 1);
        bool accelerate = AutoAccelerate || (_accelerateAction.ReadValue<float>() > 0.5f) || _uiAccel;
        bool brake = _brakeAction.ReadValue<float>() > 0.5f || _uiBrake;

        _car.SetInput(steer, accelerate, brake);
    }

    private void ResetInputState()
    {
        _uiAccel = false;
        _uiBrake = false;
        _uiSteer = 0f;
        _car.SetInput(0f, false, false);
    }
}