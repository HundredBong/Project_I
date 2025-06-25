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

    [Header("������Ʈ�� �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statPointText;

    [HideInInspector] public List<StatsSlotUI> slotUIs = new List<StatsSlotUI>();
    private void Start()
    {
        //�����ϸ� Ÿ�Կ� �°� �г� UI �˾Ƽ� ��������
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
        //UI���� ������ �ƴ϶� �������� ���ΰ�ħ�ϴ°ſ���
        //TODO : DataManager.Instance.GetLocalizedText()�� ���� �������� ��, LanguageManager.OnLanguageChanged �̺�Ʈ�� ��� ����� ���ΰ�ħ�� �ؾ���
        levelText.text = $"�� ���� : Lv.{playerStats.level}";
        statPointText.text = $"��������Ʈ {playerStats.statPoint}/{playerStats.level}";

        Debug.Log($"[PlayerStats] RefreshAllStatUIs �����, UI���� : {slotUIs.Count}");

        foreach (StatsSlotUI ui in slotUIs)
        {
            ui.Refresh();
            Debug.Log($"[PlayerStats] ���ΰ�ħ�� UI : {ui.name}");
        }
    }
}
