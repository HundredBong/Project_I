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
    [SerializeField] private Button openUpgradeButton;
    [SerializeField] private Button resetStatButton;

    [Header("업데이트할 텍스트")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statPointText;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI statButtonText;
    [SerializeField] private TextMeshProUGUI resetStatButtonText;

    [HideInInspector] public List<UIStatSlot> slotUIs = new List<UIStatSlot>();
    private void Start()
    {
        //시작하면 타입에 맞게 패널 UI 알아서 생성해줌
        foreach (StatUpgradeType type in System.Enum.GetValues(typeof(StatUpgradeType)))
        {
            GameObject obj = Instantiate(statSlotPrefab, contentRoot);
            UIStatSlot slot = obj.GetComponent<UIStatSlot>();
            slot.Init(playerStats, type);
            slotUIs.Add(slot);
        }

        openUpgradeButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIGoldUpgradePage>());
        resetStatButton.onClick.AddListener(() => GameManager.Instance.stats.ResetStats());
    }

    protected override void Awake()
    {
        base.Awake();

        if (playerStats == null)
        {
            playerStats = GameManager.Instance.stats;
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;

        playerStats.OnStatChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;

        playerStats.OnStatChanged -= Refresh;
    }

    public void Refresh()
    {
        levelText.text = $"{DataManager.Instance.GetLocalizedText("UI_MyLevel")} : {playerStats.level}";
        statPointText.text = $"{DataManager.Instance.GetLocalizedText("UI_StatPoint")} {playerStats.statPoint}/{playerStats.level}";
        resetStatButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_ResetStat")}";
        upgradeButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Upgrade")}";
        statButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Stat")}";

        foreach (UIStatSlot ui in slotUIs)
        {
            ui.Refresh();
        }
    }
}
