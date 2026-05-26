using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NavigationManager : MonoBehaviour
{

    public delegate void FadeCoverEvent(CoverState state);
    public static event FadeCoverEvent OnFadeTransitionComplete;

    public delegate void NavigationManagerEvent();
    public static event NavigationManagerEvent OnSceneLoadAndFadeComplete;


    [SerializeField] private UIDocument _fadeOverlayUI;
    [SerializeField] private float _fadeDuration;
    [SerializeField] private string _mainMenuScene;

    public float FadeDuration => _fadeDuration;

    private VisualElement _root;
    private VisualElement _fadeOverlay;
    private Button _backButton;
    private CoverState _lastRequestedState = CoverState.Unset;
    private bool _inSceneTransition;

    private static NavigationManager m_instance;
    public static NavigationManager Instance { get => m_instance; private set => m_instance = value; }

    private Coroutine _currentTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance of {typeof(NavigationManager)} already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    void Start()
    {
        RequestMainMenuScene();
    }

    private void InitializeUI()
    {
        _root = _fadeOverlayUI.rootVisualElement;
        _fadeOverlay = _root.Query<VisualElement>("FadeOverlay");
        _fadeOverlay.style.display = DisplayStyle.Flex;
        _lastRequestedState = CoverState.Cover;
        _backButton = _root.Query<Button>("Back_Btn");

        _backButton.RegisterCallback<ClickEvent>((e) =>
        {
            RequestMainMenuScene();
        });

        _backButton.style.display = DisplayStyle.None;
    }

    public Coroutine RequestCoverState(CoverState state, float delay = 0f)
    {
        _lastRequestedState = state;

        if (_currentTransition == null)
        {
            _currentTransition = StartCoroutine(ExecuteFadeTransition(state, delay));
        }

        return _currentTransition;
    }

    private IEnumerator ExecuteFadeTransition(CoverState state, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        _fadeOverlay.style.display = DisplayStyle.Flex;
        _fadeOverlay.style.opacity = state == CoverState.Cover ? 1f : 0f;
        yield return new WaitForSeconds(_fadeDuration);
        if (state == CoverState.Uncover)
        {
            _fadeOverlay.style.display = DisplayStyle.None;
        }

        OnFadeTransitionComplete?.Invoke(state);

        if (_lastRequestedState != state)
        {
            yield return ExecuteFadeTransition(_lastRequestedState);
        }
        else
        {
            _currentTransition = null;
        }
    }

    public void RequestSceneSwitch(string scene)
    {
        if (_inSceneTransition) return;
        StartCoroutine(SwitchScene(scene));
    }

    private IEnumerator SwitchScene(string scene)
    {
        _inSceneTransition = true;
        yield return RequestCoverState(CoverState.Cover);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
        float minLoadTime = 0.5f;
        float timer = 0f;

        while (asyncLoad.progress < 0.9f || timer < minLoadTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        asyncLoad.allowSceneActivation = true;
        _backButton.style.display = (scene == _mainMenuScene) ? DisplayStyle.None : DisplayStyle.Flex;
        yield return RequestCoverState(CoverState.Uncover);
        _inSceneTransition = false;
        OnSceneLoadAndFadeComplete?.Invoke();

    }

    public void RequestMainMenuScene()
    {
        RequestSceneSwitch(_mainMenuScene);
    }

    public void RequestOpenExternalLink(string url)
    {
        Application.OpenURL(url);
    }
}

public enum CoverState
{
    Unset,
    Cover,
    Uncover
}
