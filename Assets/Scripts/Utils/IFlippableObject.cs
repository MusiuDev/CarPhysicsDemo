using UnityEngine;

public interface IFlippableObject
{
    Transform TransformReference { get; }
    void Flip();
}
