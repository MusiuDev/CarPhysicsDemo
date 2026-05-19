using UnityEngine;

public class CarVisualsManager : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private DriftingVFXGroup[] tireGroups;

    void Start()
    {
        foreach (var group in tireGroups)
        {
            group.Initialize(_car);
        }
    }

    void Update()
    {
        if (!_car) return;

        foreach (var group in tireGroups)
        {
            group.Update();
        }
    }



    [System.Serializable]
    private class TireTrailPair
    {
        public Transform tireReference;
        public Transform visualTire;
        public TrailRenderer trail;
    }

    [System.Serializable]
    private class DriftingVFXGroup
    {
        [SerializeField] private TireTrailPair[] _tires;
        [SerializeField] private float _driftingStart;
        [SerializeField] private float _driftingEnd;
        [SerializeField] private float _extraTime;

        [SerializeField] private bool _steerVisualWheels;
        [SerializeField] private float _steerTurnAngle;
        [SerializeField] private float _steerTurnSpeed;

        private bool _emitting;
        private float _lastDrfitTime;
        private SmoothCarMovement _car;
        private float _currentSteer;

        public void Initialize(SmoothCarMovement car)
        {
            _car = car;
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
                if (_car.DriftingFactor >= _driftingStart)
                {
                    _emitting = true;
                    _lastDrfitTime = 0;
                }
            }
            else if (_car.DriftingFactor < _driftingEnd)
            {
                _lastDrfitTime += Time.deltaTime;
                if (_lastDrfitTime >= _extraTime)
                {
                    _emitting = false;
                }
            }

            _currentSteer = Mathf.MoveTowards(_currentSteer, _car.SteerInput * _steerTurnAngle, _steerTurnSpeed * Time.deltaTime);
        }

        private void UpdateRenderers()
        {

            foreach (var tire in _tires)
            {
                var trail = tire.trail;
                var tireRef = tire.tireReference;

                bool shouldEnable = _emitting && _car.ContacticDict[tireRef];
                trail.emitting = shouldEnable;

                if (_steerVisualWheels)
                {
                    var visualTire = tire.visualTire;
                    visualTire.transform.localEulerAngles = new Vector3(0, _currentSteer, 0);
                }
            }
        }
    }
}
