using System.Collections;
using UnityEngine;

public class CarVisualsManager : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private DriftingVFXGroup[] _tireGroups;
    [SerializeField] private TrailRenderer[] _lightTrails;

    void Start()
    {
        GameManager.OnCarPreTeleport -= HandleCarPreTeleport;
        GameManager.OnCarPreTeleport += HandleCarPreTeleport;

        GameManager.OnCarPostTeleport -= HandleCarPostTeleport;
        GameManager.OnCarPostTeleport += HandleCarPostTeleport;
        foreach (var group in _tireGroups)
        {
            group.Initialize(_car.State);
        }
    }

    void OnDestroy()
    {
        GameManager.OnCarPreTeleport -= HandleCarPreTeleport;
        GameManager.OnCarPostTeleport -= HandleCarPostTeleport;
    }

    private void HandleCarPreTeleport()
    {
        StopAllCoroutines();
        foreach (var group in _tireGroups)
        {
            group.Disable();
        }
        foreach (var trail in _lightTrails)
        {
            trail.Clear();
            trail.emitting = false;
        }
    }

    private void HandleCarPostTeleport()
    {
        StartCoroutine(WaitAndReEnable());
    }

    IEnumerator WaitAndReEnable()
    {
        yield return null;
        foreach (var group in _tireGroups)
        {
            group.Enable();
        }
        foreach (var trail in _lightTrails)
        {
            trail.Clear();
            trail.emitting = true;
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
        public ParticleSystem particles;
        public Transform visualWheel;
        public bool isPlaying = true;// true so it can be stopped at start
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
        private ICarState _carState;
        private float _currentSteer;
        private bool _canEmit;

        public void Initialize(ICarState carState)
        {
            _carState = carState;
            _canEmit = true;
            _emitting = false;

            UpdateRenderers();
        }

        public void Update()
        {
            if (!_canEmit) return;
            UpdateState();
            UpdateRenderers();
        }

        public void Disable()
        {
            _emitting = false;
            _lastDrfitTime = 0;
            _currentSteer = 0;
            _canEmit = false;
            foreach (var wheelPair in _wheelPairs)
            {
                wheelPair.trail.Clear();
            }
            UpdateRenderers();
        }

        public void Enable()
        {
            _canEmit = true;
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
                bool shouldEnable = _canEmit && _emitting && wheelPair.carWheel.InContact;

                if (wheelPair.isPlaying != shouldEnable)
                {
                    wheelPair.trail.emitting = shouldEnable;
                    if (shouldEnable)
                    {
                        wheelPair.particles.Play();
                    }
                    else
                    {
                        wheelPair.particles.Stop();
                    }
                    wheelPair.isPlaying = shouldEnable;
                }


                if (_steerVisualWheels)
                {
                    var visualTire = wheelPair.visualWheel;
                    visualTire.transform.localEulerAngles = new Vector3(0, _currentSteer, 0);
                }
            }
        }
    }
}
