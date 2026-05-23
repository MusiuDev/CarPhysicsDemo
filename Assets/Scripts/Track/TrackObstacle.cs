using UnityEngine;

public class TrackObstacle : MonoBehaviour
{
    [SerializeField] private GameObject[] _possibleProps;
    [SerializeField] private float _horizontalPositionDriftRange = 0.5f;
    [SerializeField] private float _scaleDriftRange = 0.25f;
    [SerializeField] private bool _randomizeRotation = true;

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

        GameObject randomProp = _possibleProps[Random.Range(0, _possibleProps.Length - 1)];
        Transform randomPropInstance = GameObject.Instantiate(randomProp, transform.position, transform.rotation, this.transform).transform;

        if (_horizontalPositionDriftRange > 0)
        {
            Vector2 posDrift = Random.insideUnitCircle * _horizontalPositionDriftRange;
            randomPropInstance.localPosition = posDrift.ToXZ();
        }

        if (_scaleDriftRange > 0)
        {
            float scaleDrift = 1f + Random.Range(-_scaleDriftRange, _scaleDriftRange);
            randomPropInstance.localScale = Vector3.one * scaleDrift;
        }

        if (_randomizeRotation)
        {
            float randomAngle = Random.Range(0, 360f);
            randomPropInstance.localEulerAngles = new Vector3(0, randomAngle, 0);
        }
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
}
