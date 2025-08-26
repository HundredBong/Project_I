using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConfirmPopup : UIPopup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TextMeshProUGUI _confirmButtonText;
    [SerializeField] private TextMeshProUGUI _cancelButtonText;
    [SerializeField] private TextMeshProUGUI _contentText;

    public override void Close()
    {

        base.Close();
    }

    public void Init(Action onClickConfirm, string contentText, string confirmButtonText = "UI_Confirm", string cancelButtonText = "UI_Cancel")
    {
        _confirmButtonText.text = DataManager.Instance.GetLocalizedText(confirmButtonText);
        _cancelButtonText.text = DataManager.Instance.GetLocalizedText(cancelButtonText);

        _contentText.text = DataManager.Instance.GetLocalizedText(contentText);

        _cancelButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.RemoveAllListeners();

        _confirmButton.onClick.AddListener(() =>
        {
            onClickConfirm?.Invoke();
            UIManager.Instance.PopupClose();
        });

        _cancelButton.onClick.AddListener(()=> UIManager.Instance.PopupClose());
    }
}
