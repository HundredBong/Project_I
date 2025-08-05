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
    [SerializeField] private Button _openPopupButton;

    private DungeonData _data;

    public void Init(DungeonData data)
    {
        _data = data;

        _dungeonNameText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.NameKey}");
        _dungeonDescText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.DescKey}");
        _dungeonTicketText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.TicketType}");


        _openPopupButton.onClick.RemoveAllListeners();
        _openPopupButton.onClick.AddListener(OnClickOpenPopupButton);
    }

    private void OnClickOpenPopupButton()
    {
        UIManager.Instance.PopupOpen<UIDungeonInfoPopup>().Init(_data);
    }
}
