using UnityEngine;

public class RingCheckpoint : Checkpoint
{
    [SerializeField] private Animator _ringAnimator;
    [SerializeField] private float _ringCenterHeight;
    [SerializeField] private float _ringRadius;

    private Vector3 _ringCenterWorld => transform.position + transform.up * _ringCenterHeight;
    private float _enterDirection;

    override protected bool ProcessPlayerEnter(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - _ringCenterWorld;
        _enterDirection = Vector3.Dot(transform.forward, toPlayer);

        Vector3 playerPosProyected = Vector3.ProjectOnPlane(playerTransform.position, transform.position);
        float distanceToCenter = Vector3.Distance(_ringCenterWorld, playerPosProyected);
        
        return distanceToCenter <= _ringRadius;
    }

    protected override bool ProcessPlayerExit(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - _ringCenterWorld;
        float exitDirection = Vector3.Dot(transform.forward, toPlayer);
        return exitDirection != _enterDirection;
    }

    protected override void UpdateStatePresentation()
    {
        _ringAnimator.SetInteger("State", (int)_currentState);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_ringCenterWorld, 0.25f);
        GizmoUtils.DrawCircle(_ringCenterWorld, transform.forward, _ringRadius);
    }
}
