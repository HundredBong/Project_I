using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINicknamePopup : UIPopup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private InputField _inputField;

    UniTaskCompletionSource<string> _tcs;

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
    }

    public void Init(UniTaskCompletionSource<string> tcs)
    {
        _tcs = tcs;
    }

    private void OnClickConfirmButton()
    {
        //1. 닉네임 불러오기, 
        //2. 불러온 닉네임이 null이나 default값이라면
        //3. 닉네임 인풋필드 UI 표시, 이때 await 해야 함.
        //4. 닉네임 입력후 비속어 검사
        //5. 이상없다면 파이어베이스에 await 저장후 함수 종료

        //그럼 게임매니저에서 null이면 UniTaskCompletionSource<string>하나 만들고, 
        //이 결과가 올 때까지 대기하면 될 거 같은데.
        //결과가 오면 await statSaver.SaveNickname(전달받은 닉네임)
        //마지막으로 게임 로드할 수 있게 하고,
        if (_inputField.text == "")
        {
            
        }

        _tcs.TrySetResult(_inputField.text);

        _inputField.DeactivateInputField();
    }
}
