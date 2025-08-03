using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISliderPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _icon;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Button _negativeButton;
    [SerializeField] private Button _positiveButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI _confirmButtonText;
    [SerializeField] private TextMeshProUGUI _cancelButtonText;


    private Action<int> _onConfirm;

    public void Init(string iconKey, string itemNameKey, int maxValue, Action<int> onConfirm,
        string contentText = "UI_PurchaseMessage", string confirmText = "UI_Confirm", string cancelText = "UI_Cancel")
    {
        _icon.sprite = DataManager.Instance.GetSpriteByKey(iconKey);
        _nameText.text = DataManager.Instance.GetLocalizedText(itemNameKey);

        _contentText.text = DataManager.Instance.GetLocalizedText(contentText);
        _confirmButtonText.text = DataManager.Instance.GetLocalizedText(confirmText);
        _cancelButtonText.text = DataManager.Instance.GetLocalizedText(cancelText);

        _slider.maxValue = maxValue;
        _slider.value = 1;

        _onConfirm = onConfirm;

        UpdateCountText();

        _slider.onValueChanged.RemoveAllListeners();
        _slider.onValueChanged.AddListener((value) => UpdateCountText());

        _positiveButton.onClick.RemoveAllListeners();
        _positiveButton.onClick.AddListener(() =>
        {
            if (_slider.value < _slider.maxValue)
            {
                _slider.value += 1;
            }
        });

        _negativeButton.onClick.RemoveAllListeners();
        _negativeButton.onClick.AddListener(() =>
        {
            if (_slider.value > _slider.minValue)
            {
                _slider.value -= 1;
            }
        });

        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            _onConfirm?.Invoke((int)_slider.value);
            Close();
        });

        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(() => Close());
    }

    private void UpdateCountText()
    {
        _countText.text = ((int)_slider.value).ToString();
    }

    public override void Close()
    {
        _onConfirm = null;
        _slider.onValueChanged.RemoveAllListeners();
        _positiveButton.onClick.RemoveAllListeners();
        _negativeButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.RemoveAllListeners();

        base.Close();
    }
}