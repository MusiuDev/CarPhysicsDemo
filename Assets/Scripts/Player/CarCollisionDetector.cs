using System;
using UnityEngine;

public class CarCollisionDetector : MonoBehaviour
{
    public delegate void CarCollisionEvent(Collision col);
    public event CarCollisionEvent OnCarCollisionWithObstacle;

    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private float _minImpactForceToDetect;
    [SerializeField] private float _minCollisionFactorToDetect;

    void Awake()
    {
    }


    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Resetting || !GameManager.GameActive) return;

        if ((_obstacleLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        if (collision.relativeVelocity.magnitude < _minImpactForceToDetect) return;

        Vector3 normal = GetAverageCollisionNormal(collision);
        float collisionFactor = Vector3.Dot(collision.relativeVelocity.normalized, normal);
        if (collisionFactor < _minCollisionFactorToDetect) return;

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
