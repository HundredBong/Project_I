using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDungeonPage : UIPage
{
    [SerializeField] private TextMeshProUGUI _dungeonText;
    [SerializeField] private GameObject _dungeonPrefab;
    [SerializeField] private Transform _prefabRoot;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    private void Start()
    {
        //TODO : µñ¼Å³Ê¸® ¼ø¼­ È®ÀÎ
        foreach (var item in DataManager.Instance.GetAllDungeonData().Values)
        {
            GameObject obj = Instantiate(_dungeonPrefab, _prefabRoot);
            UIDungeonSlot slot = obj.GetComponent<UIDungeonSlot>();
            slot.Init(item);
        }

        SetLocalizedText();
    }

    private void SetLocalizedText()
    {
        _dungeonText.text = DataManager.Instance.GetLocalizedText("UI_Dungeon");
    }
}
