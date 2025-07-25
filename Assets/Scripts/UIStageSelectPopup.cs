using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageSelectPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _closeButtonText;
    [SerializeField] private TMP_InputField _stageInputField;
    [SerializeField] private TextMeshProUGUI _placeholderText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TextMeshProUGUI _confirmButtonText;

    protected override void Awake()
    {
        base.Awake();

        _stageInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        _stageInputField.keyboardType = TouchScreenKeyboardType.NumberPad;
        _stageInputField.ForceLabelUpdate();
    }

    private void Start()
    {
        SetLocallizedText();
    }

    private void SetLocallizedText()
    {
        _placeholderText.text = DataManager.Instance.GetLocalizedText("UI_StageSelectPlaceholder");
        _closeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Cancel");
        _confirmButtonText.text = DataManager.Instance.GetLocalizedText("UI_Confirm");
    }

    public override void Open()
    {
        base.Open();

        _stageInputField.ActivateInputField();
        _stageInputField.text = "";

        SetLocallizedText();
    }

    private void OnEnable()
    {
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(OnClickConfirmButton);
        LanguageManager.OnLanguageChanged += SetLocallizedText;
    }

    private void OnDisable()
    {
        _confirmButton.onClick.RemoveAllListeners();
        LanguageManager.OnLanguageChanged += SetLocallizedText;
    }

    private void OnClickConfirmButton()
    {
        if (int.TryParse(_stageInputField.text, out int stage))
        {
            if (stage < 1 || stage > StageManager.Instance.MaxClearedStage)
            {
                Debug.LogWarning("[UIStageSelectPopup] 잘못된 스테이지 접근");
                return;
            }
            StageManager.Instance.GoToStage(stage);
            Close();
        }
        else
        {
            Debug.LogWarning("[UIStageSelectPopup] 잘못된 입력값");
        }

        _stageInputField.DeactivateInputField();
    }
}
