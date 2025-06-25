using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStatPage : UIPage
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject statSlotPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("업데이트할 텍스트")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statPointText;

    [HideInInspector] public List<StatsSlotUI> slotUIs = new List<StatsSlotUI>();
    private void Start()
    {
        //시작하면 타입에 맞게 패널 UI 알아서 생성해줌
        foreach (StatUpgradeType type in System.Enum.GetValues(typeof(StatUpgradeType)))
        {
            GameObject obj = Instantiate(statSlotPrefab, contentRoot);
            StatsSlotUI slot = obj.GetComponent<StatsSlotUI>();
            slot.Init(playerStats, type);
            slotUIs.Add(slot);
        }
    }

    private void OnEnable()
    {
        playerStats.OnStatChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        playerStats.OnStatChanged -= Refresh;
    }

    public void Refresh()
    {
        //UI스탯 슬롯이 아니라 페이지를 새로고침하는거였네
        //TODO : DataManager.Instance.GetLocalizedText()로 언어별로 가져와햐 함, LanguageManager.OnLanguageChanged 이벤트로 언어 변경시 새로고침도 해야함
        levelText.text = $"내 레벨 : Lv.{playerStats.level}";
        statPointText.text = $"스탯포인트 {playerStats.statPoint}/{playerStats.level}";

        Debug.Log($"[PlayerStats] RefreshAllStatUIs 실행됨, UI개수 : {slotUIs.Count}");

        foreach (StatsSlotUI ui in slotUIs)
        {
            ui.Refresh();
            Debug.Log($"[PlayerStats] 새로고침한 UI : {ui.name}");
        }
    }
}
