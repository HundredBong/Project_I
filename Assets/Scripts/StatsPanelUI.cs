using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsPanelUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public GameObject statSlotPrefab;
    public Transform contentRoot;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statPointText;

    private void Start()
    {
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
