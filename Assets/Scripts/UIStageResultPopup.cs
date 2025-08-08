using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class UIStageResultPopup : UIPopup
{
    [SerializeField] private Image _popupBackground;
    [SerializeField] private Image _resultImage;
    [SerializeField] private Color _victoryColor;
    [SerializeField] private Color _defeatColor;
    [SerializeField] private TextMeshProUGUI _currentStageText;
    [SerializeField] private TextMeshProUGUI _earnedRewardText;
    [SerializeField] private TextMeshProUGUI _retryButtonText;
    [SerializeField] private TextMeshProUGUI _exitButtonText;
    [SerializeField] private TextMeshProUGUI _autoExitText;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Transform _resultPrefabRoot;
    [SerializeField] private GameObject _defeatGroup;

    private Coroutine _autoExitCoroutine;

    private Action _onClickExitButton;
    private WaitForSeconds _wait;

    protected override void Awake()
    {
        base.Awake();
        _wait = new WaitForSeconds(1);
    }

    public void Init(DungeonType type, bool isVictory, int stage, Action onClickRetryButton, Action onClickExitButton)
    {
        _popupBackground.color = isVictory ? _victoryColor : _defeatColor;
        _defeatGroup.SetActive(isVictory == false);
        string spriteKey = isVictory ? "UI_Victory" : "UI_Defeat";
        _resultImage.sprite = DataManager.Instance.GetSpriteByKey(spriteKey);

        _currentStageText.text = $"{DataManager.Instance.GetLocalizedText($"UI_{type}")} {stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        _earnedRewardText.text = $"{DataManager.Instance.GetLocalizedText("UI_EarnedReward")}";

        string localKey = isVictory ? "UI_NextStage" : "UI_Retry";
        _retryButtonText.text = DataManager.Instance.GetLocalizedText(localKey);

        _exitButtonText.text = DataManager.Instance.GetLocalizedText("UI_Exit");

        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(() => onClickRetryButton?.Invoke());

        _exitButton.onClick.RemoveAllListeners();
        _exitButton.onClick.AddListener(() => onClickExitButton?.Invoke());

        _onClickExitButton = onClickExitButton;

        if (_autoExitCoroutine != null)
        {
            StopCoroutine(_autoExitCoroutine);
            _autoExitCoroutine = null;
        }

        _autoExitCoroutine = StartCoroutine(AutoExitCoroutine());
    }

    public Transform GetPrefabRoot()
    {
        return _resultPrefabRoot;
    }

    private IEnumerator AutoExitCoroutine()
    {
        int remain = 10;

        while (remain > 0)
        {
            _autoExitText.text = $"{(int)remain}{DataManager.Instance.GetLocalizedText("UI_AutoExit")}";
            yield return _wait;
            remain--;
        }
        _onClickExitButton?.Invoke();
    }

    public override void Close()
    {
        if (_autoExitCoroutine != null)
        {
            StopCoroutine(_autoExitCoroutine);
            _autoExitCoroutine = null;
        }

        base.Close();
    }
}
