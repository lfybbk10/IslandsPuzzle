using UnityEngine;
using TMPro;
using DG.Tweening;

public class HintUI : MonoBehaviour
{
    [HideInInspector] public bool HintOpened;

    [Header("Main UI")]
    [SerializeField] private CanvasGroup background;
    [Space]
    [SerializeField] private GameObject viewAdButton;
    [SerializeField] private GameObject hintButton;
    [Space]
    [SerializeField] private string adLoadingErrorMessageLocalizationKey = "ad_loading_error_message";
    
    [Header("Hint Panel")]
    [SerializeField] private Transform hintPanel;
    [SerializeField] private TextMeshProUGUI stepsText;
    [SerializeField] private CanvasGroup overlay;
    [Space]
    [SerializeField] private AnimatedButton previousStepButton;
    [SerializeField] private AnimatedButton nextStepButton;

    [Header("Animation")]
    [SerializeField] private float duration;
    [SerializeField] private Ease ease = Ease.OutCubic;
    [Space]
    [SerializeField] private Vector3 startRotation;

    private GameObject _hintCamera;

    private HintSystem _hintSystem;

    private AdsManager _adsManager;

    private Camera _mainCamera;

    private string _originalContentOfStepsText = string.Empty;

    private System.Func<bool> isAdViewed;
    private System.Action onAdViewed;

    public void Init(System.Func<bool> isAdViewedFunc, System.Action onAdViewedAction) {
        isAdViewed = isAdViewedFunc;
        onAdViewed = onAdViewedAction;

        _mainCamera = Camera.main;

        _hintSystem = LevelContext.Instance.HintSystem;
        _adsManager = ProjectContext.Instance.AdsManager;

        _hintCamera = BaseSceneContext.Instance.HintsRenderer.HintCamera.gameObject;

        string stepsTextLocalizationKey = stepsText.GetComponent<LocalizedText>().LocalizationKey;
        _originalContentOfStepsText = ProjectContext.Instance.Localization.GetLocalizedValue(stepsTextLocalizationKey);

        InitHintButtons();
        UpdateUI();
    }

    private void InitHintButtons(){
        _adsManager.CheckInternetConnection((isInternetReachable) => {
            hintButton.SetActive(isAdViewed.Invoke());
            viewAdButton.SetActive(isAdViewed.Invoke() == false && isInternetReachable);
        });
    }

    public void ViewAd(){
        if(_adsManager){
            string errorMessage = ProjectContext.Instance.Localization.GetLocalizedValue(adLoadingErrorMessageLocalizationKey);
            _adsManager.ShowRewardedAd(AdViewed, () => OverlayPanels.CreateNewInformationPanel(errorMessage, null));
        }
        else{
            AdViewed();
        }
    }

    private void AdViewed(){
        onAdViewed.Invoke();

        viewAdButton.SetActive(false);
        hintButton.SetActive(true);

        OpenPanel();
    }

    private void UpdateUI(){
        previousStepButton.Interactable = _hintSystem.CurrentStepIndex > 1;
        nextStepButton.Interactable = _hintSystem.CurrentStepIndex < _hintSystem.StepsCount;
        stepsText.text = string.Format(_originalContentOfStepsText, _hintSystem.CurrentStepIndex, _hintSystem.StepsCount);
    }

    public void NextStep(){
        if(_hintSystem.CurrentStepIndex < _hintSystem.StepsCount){
            AnimateOverlay(_hintSystem.NextStep);
        }
    }
    
    public void PreviousStep(){
        if(_hintSystem.CurrentStepIndex > 1){
            AnimateOverlay(_hintSystem.PreviousStep);
        }
    }

    private void AnimateOverlay(System.Action onFadeInFinished){
        overlay.DOFade(1, 0.1f).SetEase(Ease.InOutSine).OnComplete(() => {
            onFadeInFinished?.Invoke();
            UpdateUI();
            overlay.DOFade(0, 0.2f).SetEase(Ease.InOutSine);
        });
    }

    public void OpenPanel(){
        HintOpened = true;
        _mainCamera.gameObject.SetActive(false);
        _hintCamera.SetActive(true);

        hintPanel.gameObject.SetActive(true);
        hintPanel.localScale = Vector3.zero;
        hintPanel.localEulerAngles = startRotation;

        hintPanel.DOScale(Vector3.one, duration).SetEase(ease);
        hintPanel.DORotate(Vector3.zero, duration).SetEase(ease);

        _hintSystem.OnHintPanelOpened();

        ChangeBackgroundVisibility(true);
        UpdateUI();
    }

    public void ClosePanel(){
        HintOpened = false;
        _mainCamera.gameObject.SetActive(true);
        _hintCamera.SetActive(false);

        hintPanel.DOScale(Vector3.zero, duration).SetEase(ease);
        hintPanel.DORotate(startRotation, duration).SetEase(ease);

        _hintSystem.OnHintPanelClosed();

        ChangeBackgroundVisibility(false);
    }

    private void ChangeBackgroundVisibility(bool visible, float delay = 0){
        background.gameObject.SetActive(true);
        
        background.DOFade(visible ? 1 : 0, 0.25f).SetDelay(delay).OnComplete(() => {
            if(visible == false) 
                background.gameObject.SetActive(false);
        });
    }
}
