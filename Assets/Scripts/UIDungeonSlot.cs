using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIDungeonSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dungeonNameText;
    [SerializeField] private TextMeshProUGUI _dungeonDescText;
    [SerializeField] private TextMeshProUGUI _dungeonTicketText;
    [SerializeField] private Image _dungeonImage;
    [SerializeField] private Image _dungeonRewardImage;
    [SerializeField] private Button _openPopupButton;

    private DungeonData _data;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    public void Init(DungeonData data)
    {
        _data = data;

        _dungeonNameText.text = DataManager.Instance.GetLocalizedText($"{_data.NameKey}");
        _dungeonDescText.text = DataManager.Instance.GetLocalizedText($"{_data.DescKey}");
        _dungeonTicketText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.TicketType}");
        _dungeonImage.sprite = DataManager.Instance.GetSpriteByKey($"{_data.DungeonSpriteKey}");
        _dungeonRewardImage.sprite = DataManager.Instance.GetSpriteByKey($"{_data.MainRewardKey}");

        _openPopupButton.onClick.RemoveAllListeners();
        _openPopupButton.onClick.AddListener(OnClickOpenPopupButton);
    }

    private void SetLocalizedText()
    {
        if (_data == null) return;
        _dungeonNameText.text = DataManager.Instance.GetLocalizedText($"{_data.NameKey}");
        _dungeonDescText.text = DataManager.Instance.GetLocalizedText($"{_data.DescKey}");
        _dungeonTicketText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.TicketType}");
    }

    private void OnClickOpenPopupButton()
    {
        UIManager.Instance.PopupOpen<UIDungeonInfoPopup>().Init(_data);
    }
}
