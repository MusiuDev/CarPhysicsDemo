using UnityEngine;

public class CheckpointGroupFlippableObject : MonoBehaviour, IFlippableObject
{
    public Transform TransformReference => this.transform;
    public void Flip()
    {
        Vector3 pos = transform.localPosition;
        Vector3 rot = transform.localEulerAngles;

        pos.x *= -1f;
        rot.y *= -1f;

        transform.localPosition = pos;
        transform.localEulerAngles = rot;
    }
}