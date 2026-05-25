using UnityEngine;

public class CarCollisionDetector : MonoBehaviour
{
    public delegate void CarCollisionEvent(Collision col);
    public event CarCollisionEvent OnCarCollisionWithObstacle;

    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private float _minImpactForceToDetect;
    [SerializeField] private float _minCollisionFactorToDetect;

    private bool _isResetting;

    void Awake()
    {
        InfiniteDriftGameManager.OnCarResetStarted += HandleResetStarted;
        InfiniteDriftGameManager.OnCarResetCompleted += HandleResetCompleted;
    }

    private void HandleResetStarted()
    {
        _isResetting = true;
    }

    private void HandleResetCompleted()
    {
        _isResetting = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_isResetting) return;
        
        if ((_obstacleLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        if (collision.relativeVelocity.magnitude < _minImpactForceToDetect) return;

        Vector3 normal = GetAverageCollisionNormal(collision);
        float collisionFactor = Vector3.Dot(collision.relativeVelocity.normalized, normal);
        if (collisionFactor < _minCollisionFactorToDetect) return;

        Debug.Log($"Detected collision at {collision.relativeVelocity.magnitude} force, and {collisionFactor} factor");

        OnCarCollisionWithObstacle?.Invoke(collision);
    }

    private Vector3 GetAverageCollisionNormal(Collision collision)
    {
        Vector3 result = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            result += collision.contacts[i].normal;
        }

        return result.normalized;
    }
}
