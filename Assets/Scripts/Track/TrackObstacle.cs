using UnityEngine;

public class TrackObstacle : MonoBehaviour, IFlippableObject
{
    [SerializeField] private GameObjectPoolable[] _possibleProps;
    [SerializeField] private float _horizontalPositionDriftRange = 0.5f;
    [SerializeField] private float _scaleDriftRange = 0.25f;
    [SerializeField] private bool _randomizeRotation = true;
    [SerializeField] private bool _flipPosition = true;
    [SerializeField] private bool _flipRotation = true;

    private GameObjectPoolable _currentProp;
    private Vector2 _currentPosDrift;
    private float _currentRandomScale;
    private float _currentRandomRot;

    void Start()
    {
        Randomize();
    }

    void Randomize()
    {
        if (_possibleProps == null || _possibleProps.Length == 0)
        {
            Debug.LogWarning("Trying to randomize empty prop");
            return;
        }

        GameObjectPoolable randomProp = _possibleProps[Random.Range(0, _possibleProps.Length - 1)];
        _currentProp = DynamicPoolProvider.Get(randomProp);

        if (_horizontalPositionDriftRange > 0)
        {
            _currentPosDrift = Random.insideUnitCircle * _horizontalPositionDriftRange;
        }

        if (_scaleDriftRange > 0)
        {
            _currentRandomScale = 1f + Random.Range(-_scaleDriftRange, _scaleDriftRange);
        }

        if (_randomizeRotation)
        {
            _currentRandomRot = Random.Range(0, 360f);
        }

        UpdatePropTransform();
    }

    private void UpdatePropTransform()
    {
        if (!_currentProp) return;
        _currentProp.transform.position = transform.position + _currentPosDrift.ToXZ();
        _currentProp.transform.localScale = Vector3.one * _currentRandomScale;
        _currentProp.transform.rotation = Quaternion.AngleAxis(_currentRandomRot, transform.up);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        GizmoUtils.DrawCircle(transform.position, transform.up, _horizontalPositionDriftRange);
        if (_possibleProps != null && _possibleProps.Length > 0 && _possibleProps[0] != null)
        {
            MeshFilter meshFilter = _possibleProps[0].GetComponent<MeshFilter>();
            if (meshFilter && meshFilter.sharedMesh)
            {
                Gizmos.color = new Color(0.000f, 1.000f, 0.000f, 0.5f);
                Gizmos.DrawMesh(meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale);
            }
        }
    }

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

        UpdatePropTransform();
    }
}
