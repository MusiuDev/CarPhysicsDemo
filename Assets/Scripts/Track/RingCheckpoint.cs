using UnityEngine;

public class RingCheckpoint : Checkpoint
{
    [SerializeField] private Animator _ringAnimator;
    [SerializeField] private float _ringCenterHeight;
    [SerializeField] private float _ringRadius;

    private Vector3 _ringCenterWorld => transform.position + transform.up * _ringCenterHeight;
    private int _enterDirection;

    override protected bool ProcessPlayerEnter(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - _ringCenterWorld;
        _enterDirection = Mathf.RoundToInt(Mathf.Sign(Vector3.Dot(transform.forward, toPlayer.normalized)));

        Plane p = new Plane(transform.forward, transform.position);

        Vector3 playerPosProyected = p.ClosestPointOnPlane(playerTransform.position);
        float distanceToCenter = Vector3.Distance(_ringCenterWorld, playerPosProyected);
        return distanceToCenter <= _ringRadius;
    }

    protected override bool ProcessPlayerExit(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - _ringCenterWorld;
        int exitDirection = Mathf.RoundToInt(Mathf.Sign(Vector3.Dot(transform.forward, toPlayer.normalized)));
        Debug.Log($"Ring Checkpoint Exit Result Enter: {_enterDirection} - Exit: {exitDirection}");
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
