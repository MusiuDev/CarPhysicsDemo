using System.Collections.Generic;
using UnityEngine;

public class CarVisualsManager : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private TireTrailPair[] _backTrailRenderers;
    [SerializeField] private TireTrailPair[] _frontTrailRenderers;
    [SerializeField] private float _backDriftingStart;
    [SerializeField] private float _backDriftingEnd;
    [SerializeField] private float _frontDriftingStart;
    [SerializeField] private float _frontDriftingEnd;

    private bool _driftingBack = false;
    private bool _driftingFront = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateDriftingStates();
        for (int i = 0; i < _backTrailRenderers.Length; i++)
        {
            var trail = _backTrailRenderers[i].trail;
            var tire = _backTrailRenderers[i].tire;

            bool shouldEnable = _driftingBack && _car.ContacticDict[tire];

            trail.emitting = shouldEnable;
        }

        for (int i = 0; i < _frontTrailRenderers.Length; i++)
        {
            var trail = _frontTrailRenderers[i].trail;
            var tire = _frontTrailRenderers[i].tire;

            bool shouldEnable = _driftingFront && _car.ContacticDict[tire];

            trail.emitting = shouldEnable;
        }
    }

    private void UpdateDriftingStates()
    {
        if (!_driftingBack)
        {
            if (_car.DriftingFactor >= _backDriftingStart)
            {
                _driftingBack = true;
            }
        }
        else
        {
            if (_car.DriftingFactor < _backDriftingEnd)
            {
                _driftingBack = false;
            }
        }

        if (!_driftingFront)
        {
            if (_car.DriftingFactor >= _frontDriftingStart)
            {
                _driftingFront = true;
            }
        }
        else
        {
            if (_car.DriftingFactor < _frontDriftingEnd)
            {
                _driftingFront = false;
            }
        }
    }

    [System.Serializable]
    private class TireTrailPair
    {
        public Transform tire;
        public TrailRenderer trail;
    }
}
