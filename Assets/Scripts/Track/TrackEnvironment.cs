using System;
using UnityEngine;

public class TrackEnvironment : MonoBehaviour
{
    [SerializeField] private TrackManager _trackManager;
    [SerializeField] private Transform _ground;

    void Awake()
    {
        TrackManager.OnTrackUpdated += HandleTrackUpodated;
    }

    private void HandleTrackUpodated()
    {
        Vector3 first = _trackManager.CurrentChain.First.entryPosition;
        Vector3 last = _trackManager.CurrentChain.Last.exitPosition;

        Vector3 center = (first + last) * 0.5f;
        Vector3 direction = last - first;

        _ground.position = center;
        _ground.forward = direction.normalized;
    }
}
