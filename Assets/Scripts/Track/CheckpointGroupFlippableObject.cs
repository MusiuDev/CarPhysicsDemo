using UnityEngine;

public class CheckpointGroupFlippableObject : MonoBehaviour, IFlippableObject
{
    [SerializeField] private bool _flipPosition = true;
    [SerializeField] private bool _flipRotation = true;

    public Transform TransformReference => this.transform;
    public void Flip()
    {
        if (_flipPosition)
        {
            Vector3 pos = transform.localPosition;
            pos.x *= -1f;
            transform.localPosition = pos;
        }

        if (_flipRotation)
        {
            Vector3 rot = transform.localEulerAngles;
            transform.localEulerAngles = rot;
            rot.y *= -1f;
        }
    }
}