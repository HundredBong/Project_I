using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOfflineRewardPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _offlineRewardText;
    [SerializeField] private TextMeshProUGUI _offlineMinutesText;
    [SerializeField] private TextMeshProUGUI _maxOfflineTimeText;
    [SerializeField] private TextMeshProUGUI _rewardListText;
    [SerializeField] private TextMeshProUGUI _getRewardButtonText;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private Button _getButton;

    private List<UIRewardSlot> _rewardSlots = new List<UIRewardSlot>(8);
    private Action _onClickGetButton;

    public override void Open()
    {
        base.Open();
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;

        _getButton.onClick.AddListener(OnClickGetButton);
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;

        _onClickGetButton = null;
        _getButton.onClick.RemoveListener(OnClickGetButton);
    }

    private void Start()
    {
        SetLocalizedText();
    }

    private void SetLocalizedText()
    {
        _offlineRewardText.text = $"{DataManager.Instance.GetLocalizedText("UI_OfflineReward")}";
        _maxOfflineTimeText.text = $"{DataManager.Instance.GetLocalizedText("UI_MaxOfflineTime")}";
        _rewardListText.text = $"{DataManager.Instance.GetLocalizedText("UI_RewardList")}";
        _getRewardButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_GetReward")}";
    }

    public void Init(int offlineMinutes, Action onClickGetButton, string[] rewardIconKeys, int[] amounts)
    {
        int hours = offlineMinutes / 60;
        int remainMinutes = offlineMinutes % 60;

        if (hours > 0)
        {
            //n시간 n분
            _offlineMinutesText.text = $"{hours}{DataManager.Instance.GetLocalizedText("UI_Hours")} {offlineMinutes}{DataManager.Instance.GetLocalizedText("UI_Minutes")}";
        }
        else
        {
            //n분
            _offlineMinutesText.text = $"{offlineMinutes}{DataManager.Instance.GetLocalizedText("UI_Minutes")}";
        }

        _onClickGetButton = onClickGetButton;

        for (int i = 0; i < rewardIconKeys.Length; i++)
        {
            UIRewardSlot slot = ObjectPoolManager.Instance.uiPool.GetRewardSlot();
            slot.Init(rewardIconKeys[i], amounts[i]);
            slot.transform.SetParent(_contentRoot);
            _rewardSlots.Add(slot);
        }
    }

    public override void Close()
    {
        foreach (UIRewardSlot slot in _rewardSlots)
        {
            slot.transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(slot);
        }

        base.Close();
    }

    private void OnClickGetButton()
    {
        _onClickGetButton?.Invoke();
        Close();
    }
}
