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

    protected static bool _resetting;
    public static bool Resetting => _resetting;

    protected static bool _gameActive;
    public static bool GameActive => _gameActive;

    private void Awake()
    {
        NavigationManager.OnSceneLoadAndFadeComplete += HandleTransitionComplete;
        HandleAwake();
    }

    protected virtual void HandleAwake() { /* Empty by design */ }

    private void Start()
    {
        HandleStart();
        if (!NavigationManager.Instance) HandleTransitionComplete();
    }

    protected virtual void HandleStart() { /* Empty by design */ }

    private void OnDestroy()
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

    protected virtual void CarResetStarted() { /* Empty by design */ }
    protected virtual void CarResetCompleted() { /* Empty by design */ }
    protected virtual void CarPreTeleport() { /* Empty by design */ }
    protected virtual void CarPostTeleport() { /* Empty by design */ }
}
