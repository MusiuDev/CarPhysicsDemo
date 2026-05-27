using UnityEngine;

public class SmoothCarCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _carTransform;
    [SerializeField] private Camera _camera;
    [SerializeField] private CameraSettings _landscapeSettings;
    [SerializeField] private CameraSettings _portraitSetttings;

    private float _currentYAngle;

    void LateUpdate()
    {
        FollowTarget();
    }

    public void FollowTarget()
    {
        if (!_carTransform || !_camera) return;
        CameraSettings currentSettings = CameraSettings.Lerp(_landscapeSettings, _portraitSetttings, _camera.aspect);

        _currentYAngle = Mathf.LerpAngle(_currentYAngle, _carTransform.eulerAngles.y, currentSettings.rotationDamping * Time.deltaTime);
        Vector3 angledPos = _carTransform.position + Quaternion.Euler(currentSettings.angle, _currentYAngle, 0) * (Vector3.up * currentSettings.distance);

        transform.position = angledPos;

        transform.LookAt(_carTransform.position, Vector3.up);

        transform.Rotate(currentSettings.offsetAngle, 0, 0);
    }

    [System.Serializable]
    public struct CameraSettings
    {
        public float idealAspectRatio;
        public float distance;
        public float angle;
        public float offsetAngle;
        public float rotationDamping;

        public static CameraSettings Lerp(CameraSettings from, CameraSettings to, float aspect)
        {
            float t = aspect.Remap(from.idealAspectRatio, to.idealAspectRatio, 0, 1);
            return new CameraSettings
            {
                distance = Mathf.Lerp(from.distance, to.distance, t),
                angle = Mathf.Lerp(from.angle, to.angle, t),
                offsetAngle = Mathf.Lerp(from.offsetAngle, to.offsetAngle, t),
                rotationDamping = Mathf.Lerp(from.rotationDamping, to.rotationDamping, t),
            };
        }
    }
}