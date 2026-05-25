using System.Collections;
using UnityEngine;

public class MainMenuCarRotator : MonoBehaviour
{
    [SerializeField] private Vector3 _initialRotationSpeed;
    [SerializeField] private Vector3 _targetRotationSpeed;
    [SerializeField] private float _transitionTime;
    [SerializeField] private TransformRotator _rotator;

    void Start()
    {
        StartCoroutine(Prewarm());
    }

    void Update()
    {

    }

    private IEnumerator Prewarm()
    {
        float t = 0f;
        while (t < _transitionTime)
        {
            yield return null;
            t += Time.deltaTime;
            float normalizedT = t / _transitionTime;
            _rotator.rotationSpeed = Vector3.Lerp(_initialRotationSpeed, _targetRotationSpeed, normalizedT);
        }
        _rotator.rotationSpeed = _targetRotationSpeed;
    }
}
