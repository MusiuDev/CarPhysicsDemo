using UnityEngine;

public class CarStuckDetection : MonoBehaviour
{
    public delegate void CarStuckEvent();
    public event CarStuckEvent OnCarStuck;

    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private float _flippedTimeThreshold = 0.5f;
    [SerializeField] private float _flippedSpeedThreshold = 1f;

    [SerializeField] private float _stoppedTimeThreshold = 2f;
    [SerializeField] private float _stoppedSpeedThreshold = 1f;

    private bool _isFlipped;
    private float _flippedCurrentTime;
    private bool _isStopped;
    private float _stoppedCurrentTime;

    private ICarState _state;

    void Awake()
    {
        GameManager.OnCarResetCompleted += HandleResetCompleted;
    }

    void Start()
    {
        _state = _car.State;
    }

    private void HandleResetCompleted()
    {
        _isFlipped = false;
        _flippedCurrentTime = 0f;
        _isStopped = false;
        _stoppedCurrentTime = 0f;
    }

    void Update()
    {
        if (GameManager.Resetting || !GameManager.GameActive) return;
        UpdateFlippedStatus();
        UpdateStoppedStatus();
    }

    private void UpdateFlippedStatus()
    {
        if (CheckCarFlipped())
        {
            if (!_isFlipped)
            {
                _isFlipped = true;
                _flippedCurrentTime = 0f;
            }
        }
        else
        {
            _isFlipped = false;
            _flippedCurrentTime = 0f;
        }

        if (_isFlipped)
        {
            _flippedCurrentTime += Time.deltaTime;
            if (_flippedCurrentTime >= _flippedTimeThreshold)
            {
                OnCarStuck?.Invoke();
            }
        }
    }

    private bool CheckCarFlipped()
    {
        float dot = Vector3.Dot(_state.RbUp, Vector3.up);
        if (dot > 0f) return false;
        if (_state.Speed > _flippedSpeedThreshold) return false;

        return true;
    }

    private void UpdateStoppedStatus()
    {
        if (CheckCarStopped())
        {
            if (!_isStopped)
            {
                _isStopped = true;
                _stoppedCurrentTime = 0f;
            }
        }
        else
        {
            _isStopped = false;
            _stoppedCurrentTime = 0f;
        }

        if (_isStopped)
        {
            _stoppedCurrentTime += Time.deltaTime;
            if (_stoppedCurrentTime >= _stoppedTimeThreshold)
            {
                OnCarStuck?.Invoke();
            }
        }
    }

    private bool CheckCarStopped()
    {
        return _state.Speed < _stoppedSpeedThreshold && (_state.AccelerateInput || _state.BrakeInput);
    }
}
