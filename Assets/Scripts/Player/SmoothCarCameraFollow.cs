using UnityEngine;

public class SmoothCarCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _carTransform;
    [SerializeField] private float _distance = 6.4f;
    [SerializeField] private float _height = 1.4f;
    [SerializeField] private float _heightOffset = 2f;
    [SerializeField] private float _rotationDamping = 3.0f;

    private float _currentAngle;

    void LateUpdate()
    {
        FollowTarget(_carTransform);
    }

    private void FollowTarget(Transform targetTransform)
    {
        if (!targetTransform) return;
        
        _currentAngle = Mathf.LerpAngle(_currentAngle, _carTransform.eulerAngles.y, _rotationDamping * Time.deltaTime);
        Vector3 pos = targetTransform.position - Quaternion.Euler(0, _currentAngle, 0) * Vector3.forward * _distance;

        pos.y = targetTransform.position.y + _height;
        transform.position = pos;

        transform.LookAt(targetTransform.position + Vector3.up * _heightOffset, Vector3.up);
    }

}
