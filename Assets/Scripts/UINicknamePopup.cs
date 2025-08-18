using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINicknamePopup : UIPopup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _checkButton;
    [SerializeField] private TMP_InputField _inputField;

    UniTaskCompletionSource<string> _tcs;
    private string _nickname;

    protected override void Awake()
    {
        base.Awake();

        _inputField.keyboardType = TouchScreenKeyboardType.Default;
        _inputField.ForceLabelUpdate();
    }

    public override void Open()
    {
        base.Open();

        _inputField.ActivateInputField();
        _inputField.text = "";

        _checkButton.onClick.RemoveAllListeners();
        _checkButton.onClick.AddListener(OnClickCheckButton);

        _confirmButton.interactable = false;
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(OnClickConfirmButton);
    }

    public void Init(UniTaskCompletionSource<string> tcs)
    {
        _tcs = tcs;
    }

    private void OnClickCheckButton()
    {
        _nickname = _inputField.text;
        _confirmButton.interactable = false;

        if (FwordFilter.TryFindFword(_nickname))
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_닉네임에 비속어 있음");
            return;
        }
        else if (string.IsNullOrWhiteSpace(_nickname))
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_닉네임에 공백 있음");          
            return;
        }
        else if(_nickname.Length > 8)
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_닉네임 길이는 최대 8글자");
            return;
        }
        else if (_nickname.Length < 2)
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_닉네임 길이는 최소 2글자");
            return;
        }

        ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_사용 가능한 닉네임임");
        _confirmButton.interactable = true;
    }

    private void OnClickConfirmButton()
    {
        UIManager.Instance.PopupOpen<UIConfirmPopup>().Init(() =>
        {
            _tcs.TrySetResult(_nickname);
            _inputField.DeactivateInputField();
            Close();
        },
        "UI_EnsureNickname");
    }
}
