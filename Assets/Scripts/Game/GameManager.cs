using System.Collections;
using UnityEngine;

public abstract class GameManager : MonoBehaviour
{
    public delegate void GameManagerEvent();
    public static event GameManagerEvent OnGameStarted;
    protected void RaiseOnGameStarted() => OnGameStarted?.Invoke();

    public static event GameManagerEvent OnGamePaused;
    protected void RaiseOnGamePaused() => OnGamePaused?.Invoke();

    public static event GameManagerEvent OnGameUnpaused;
    protected void RaiseOnGameUnpaused() => OnGameUnpaused?.Invoke();

    public static event GameManagerEvent OnGameStopped;
    protected void RaiseOnGameStopped() => OnGameStopped?.Invoke();

    public static event GameManagerEvent OnCarResetStarted;
    public static event GameManagerEvent OnCarResetCompleted;
    public static event GameManagerEvent OnCarPreTeleport;
    public static event GameManagerEvent OnCarPostTeleport;

    [SerializeField] protected SmoothCarMovement _car;
    [SerializeField] protected CarInputController _inputController;
    [SerializeField] protected CarCollisionDetector _carCollision;
    [SerializeField] protected CarStuckDetection _carStuck;
    [SerializeField] protected CarStatsCotroller _carStats;
    [SerializeField] protected Bounds _killBounds;

    protected static bool _resetting;
    public static bool Resetting => _resetting;

    protected static bool _gameActive;
    public static bool GameActive => _gameActive;

    protected Vector3 _safeRevivePosition;
    protected Quaternion _safeReviveRotation;

    protected void Awake()
    {
        NavigationManager.OnSceneLoadAndFadeComplete += HandleTransitionComplete;
        HandleAwake();
    }

    protected virtual void HandleAwake() { /* Empty by design */ }

    protected void Start()
    {
        HandleStart();
        if (!NavigationManager.Instance) HandleTransitionComplete();
    }

    protected virtual void HandleStart() { /* Empty by design */ }

    protected void OnDestroy()
    {
        NavigationManager.OnSceneLoadAndFadeComplete -= HandleTransitionComplete;
        HandleDestroy();
    }

    protected virtual void HandleDestroy() { /* Empty by design */ }

    protected virtual void HandleTransitionComplete() { /* Empty by design */ }

    protected void RequestResetcar(Vector3 position, Quaternion rotation, float delay = 0f)
    {
        if (_resetting) return;
        StartCoroutine(ResetCar(position, rotation, delay));
    }

    void Update()
    {
        if (_car)
        {
            //I'm not using "Bounds.Contains" so the bounds can define only some dimensions as valid if required
            bool outside =
            _killBounds.size.x > Mathf.Epsilon && !_car.transform.position.x.Between(_killBounds.min.x, _killBounds.max.x) ||
            _killBounds.size.y > Mathf.Epsilon && !_car.transform.position.y.Between(_killBounds.min.y, _killBounds.max.y) ||
            _killBounds.size.z > Mathf.Epsilon && !_car.transform.position.z.Between(_killBounds.min.z, _killBounds.max.z);

            if (outside)
            {
                RequestResetcar(_safeRevivePosition, _safeReviveRotation, 0f);
            }
        }
    }

    private IEnumerator ResetCar(Vector3 position, Quaternion rotation, float delay)
    {
        _resetting = true;
        CarResetStarted();
        OnCarResetStarted?.Invoke();
        yield return NavigationManager.Instance ? NavigationManager.Instance.RequestCoverState(CoverState.Cover, delay) : new WaitForSeconds(delay);
        CarPreTeleport();
        OnCarPreTeleport?.Invoke();
        yield return null;
        _carStats.ClearStatusEffects();
        _car.ResetToPositionAndRotation(position, rotation);
        yield return NavigationManager.Instance ? NavigationManager.Instance.RequestCoverState(CoverState.Uncover) : null;
        CarPostTeleport();
        OnCarPostTeleport?.Invoke();
        CarResetCompleted();
        OnCarResetCompleted?.Invoke();
        _resetting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_killBounds.center, _killBounds.size);
    }

    protected virtual void CarResetStarted() { /* Empty by design */ }
    protected virtual void CarResetCompleted() { /* Empty by design */ }
    protected virtual void CarPreTeleport() { /* Empty by design */ }
    protected virtual void CarPostTeleport() { /* Empty by design */ }
}
