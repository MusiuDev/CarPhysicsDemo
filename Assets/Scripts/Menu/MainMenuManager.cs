using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private UIDocument _mainMenuDocument;
    [SerializeField] private string _driftScene;
    [SerializeField] private string _stuntsScene;
    [SerializeField] private string _githubLink;
    [SerializeField] private string _portfolioLink;

    VisualElement _root;
    void Start()
    {
        InitializeUI();
        NavigationManager.Instance.RequestCoverState(CoverState.Uncover, 0.5f);
    }

    private void InitializeUI()
    {
        _root = _mainMenuDocument.rootVisualElement;

        Button playDriftButton = _root.Query<Button>("PlayDrift_Btn");
        playDriftButton.RegisterCallback<ClickEvent>((e) =>
        {
            NavigationManager.Instance.RequestSceneSwitch(_driftScene);
        });

        Button playStuntsButton = _root.Query<Button>("PlayStunts_Btn");
        playStuntsButton.RegisterCallback<ClickEvent>((e) =>
        {
            NavigationManager.Instance.RequestSceneSwitch(_stuntsScene);
        });

        Button githubButton = _root.Query<Button>("Github_Btn");
        githubButton.RegisterCallback<ClickEvent>((e) =>
        {
            NavigationManager.Instance.RequestOpenExternalLink(_githubLink);
        });

        Button portfolioButton = _root.Query<Button>("Portfolio_Btn");
        portfolioButton.RegisterCallback<ClickEvent>((e) =>
        {
            NavigationManager.Instance.RequestOpenExternalLink(_portfolioLink);
        });
    }
}
