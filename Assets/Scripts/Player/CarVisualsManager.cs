using UnityEngine;
using UnityEngine.Serialization;

public class CarVisualsManager : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private DriftingVFXGroup[] _tireGroups;

    void Start()
    {
        foreach (var group in _tireGroups)
        {
            group.Initialize(_car.State);
        }
    }

    void Update()
    {
        if (!_car) return;

        foreach (var group in _tireGroups)
        {
            group.Update();
        }
    }

    [System.Serializable]
    private class WheelTrailPair
    {
        public CarWheel carWheel;
        public TrailRenderer trail;
        public Transform visualWheel;
    }

    [System.Serializable]
    private class DriftingVFXGroup
    {
        [SerializeField] private WheelTrailPair[] _wheelPairs;
        [SerializeField] private float _driftingStart;
        [SerializeField] private float _driftingEnd;
        [SerializeField] private float _extraTime;

        [SerializeField] private bool _steerVisualWheels;
        [SerializeField] private float _steerTurnAngle;
        [SerializeField] private float _steerTurnSpeed;

        private bool _emitting;
        private float _lastDrfitTime;
        private SmoothCarMovement.ICarState _carState;
        private float _currentSteer;

        public void Initialize(SmoothCarMovement.ICarState carState)
        {
            _carState = carState;
        }

        public void Update()
        {
            UpdateState();
            UpdateRenderers();
        }

        private void UpdateState()
        {
            if (!_emitting)
            {
                if (_carState.DriftingFactor >= _driftingStart)
                {
                    _emitting = true;
                    _lastDrfitTime = 0;
                }
            }
            else if (_carState.DriftingFactor < _driftingEnd)
            {
                _lastDrfitTime += Time.deltaTime;
                if (_lastDrfitTime >= _extraTime)
                {
                    _emitting = false;
                }
            }

            _currentSteer = Mathf.MoveTowards(_currentSteer, _carState.SteerInput * _steerTurnAngle, _steerTurnSpeed * Time.deltaTime);
        }

        private void UpdateRenderers()
        {

            foreach (var wheelPair in _wheelPairs)
            {
                bool shouldEnable = _emitting && wheelPair.carWheel.InContact;
                wheelPair.trail.emitting = shouldEnable;

                if (_steerVisualWheels)
                {
                    var visualTire = wheelPair.visualWheel;
                    visualTire.transform.localEulerAngles = new Vector3(0, _currentSteer, 0);
                }
            }
        }
    }
}
