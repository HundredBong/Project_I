using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStatPage : UIPage
{
    [SerializeField] private Button openButton;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject statSlotPrefab;
    [SerializeField] private Transform contentRoot;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statPointText;

    protected override void Awake()
    {
        base.Awake();
        if (openButton != null)
        {
            openButton.onClick.AddListener(() =>
            {
                UIManager.Instance.PageOpen<UIStatPage>();
            });
        }

    }

    private void Start()
    {
        //시작하면 타입에 맞게 패널 UI 알아서 생성해줌
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            GameObject obj = Instantiate(statSlotPrefab, contentRoot);
            StatsSlotUI slot = obj.GetComponent<StatsSlotUI>();
            slot.Init(playerStats, type);
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
        levelText.text = $"내 레벨 : Lv.{playerStats.level}";
        statPointText.text = $"스탯포인트 {playerStats.statPoint}/{playerStats.level}";
    }
}
