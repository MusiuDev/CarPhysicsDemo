using UnityEngine;

public class CarWheel : MonoBehaviour
{
    public bool InContact { get; private set; }
    public float ContactSlope { get; private set; }

    // This is done as a get function to allow for a single source of truth for visualizers and testing
    public Ray GetRay()
    {
        return new Ray(transform.position, -transform.up);
    }

    public void UpdateContact(float raycastDistance)
    {
        Ray ray = GetRay();
        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance))
        {
            InContact = true;
            ContactSlope = Mathf.Clamp01(Vector3.Dot(hitInfo.normal, Vector3.up));
        }
        else
        {
            InContact = false;
            ContactSlope = -1f;
        }
    }
}
