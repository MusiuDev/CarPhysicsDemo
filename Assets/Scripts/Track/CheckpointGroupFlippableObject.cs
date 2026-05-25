using UnityEngine;

public class CheckpointGroupFlippableObject : MonoBehaviour, IFlippableObject
{
    [SerializeField] private bool _flipPosition = true;
    [SerializeField] private bool _flipRotation = true;

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
            rot.y *= -1f;
            transform.localEulerAngles = rot;
        }
    }
}