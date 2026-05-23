using UnityEngine;

public class CarCollisionDetector : MonoBehaviour
{
    public delegate void CarCollisionEvent(Collision col);
    public event CarCollisionEvent OnCarCollisionWithObstacle;

    [SerializeField] private LayerMask _obstacleLayers;

    void OnCollisionEnter(Collision collision)
    {
        if ((_obstacleLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        OnCarCollisionWithObstacle?.Invoke(collision);
    }
}
