using System;
using UnityEngine;

public class InfiniteDriftGameManager : MonoBehaviour
{
    [SerializeField] private SmoothCarMovement _car;
    [SerializeField] private CarCollisionDetector _carCollision;

    private Vector3 _revivePosition;
    private Quaternion _reviveRotation;

    void Awake()
    {
        CheckpointGroup.OnCheckpointGroupCleared += HandleCheckpointGroupCleared;
        _carCollision.OnCarCollisionWithObstacle += HandleCarCollision;
    }

    private void HandleCarCollision(Collision col)
    {
        _car.ResetToPositionAndRotation(_revivePosition, _reviveRotation);
    }

    private void HandleCheckpointGroupCleared(CheckpointGroup group)
    {
        if (group.ExitPoint)
        {
            _revivePosition = group.ExitPoint.position;
            _reviveRotation = group.ExitPoint.rotation;
        }
    }
}
