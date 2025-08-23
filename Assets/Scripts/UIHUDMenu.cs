using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHUDMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button statButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button dungeonButton;
    [SerializeField] private Button _rankButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _sleepButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI shopText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI dungeonButtonText;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI stageGoldText;
    [SerializeField] private TextMeshProUGUI stageDiamondText;
    [SerializeField] private TextMeshProUGUI shopGoldText;
    [SerializeField] private TextMeshProUGUI shopDiamondText;

    private void OnEnable()
    {
        shopButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIShopPage>());
        statButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIGoldUpgradePage>());
        skillButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UISkillPage>());
        inventoryButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIInventoryPage>());
        dungeonButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIDungeonPage>());
        _rankButton.onClick.AddListener(() => UIManager.Instance.PopupOpen<UIRankingPopup>());
        _optionsButton.onClick.AddListener(() => UIManager.Instance.PopupOpen<UIOptionPopup>());
        _sleepButton.onClick.AddListener(()=> UIManager.Instance.PopupOpen<UISleepPopup>());

        LanguageManager.OnLanguageChanged += RefreshLanguage;
        GameManager.Instance.stats.OnCurrencyChanged += RefreshCurrency;
    }

    private void OnDisable()
    {
        shopButton.onClick.RemoveAllListeners();
        statButton.onClick.RemoveAllListeners();
        skillButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.RemoveAllListeners();
        dungeonButton.onClick.RemoveAllListeners();
        _rankButton.onClick.RemoveAllListeners();
        _optionsButton.onClick.RemoveAllListeners();

        LanguageManager.OnLanguageChanged -= RefreshLanguage;
        GameManager.Instance.stats.OnCurrencyChanged -= RefreshCurrency;
    }

    private void Start()
    {
        RefreshCurrency();
        RefreshLanguage();
    }

    public void RefreshLanguage()
    {
        shopText.text = DataManager.Instance.GetLocalizedText("HUD_Shop");
        statText.text = DataManager.Instance.GetLocalizedText("HUD_Stat");
        skillText.text = DataManager.Instance.GetLocalizedText("HUD_Skill");
        inventoryText.text = DataManager.Instance.GetLocalizedText("HUD_Inventory");
        dungeonButtonText.text = DataManager.Instance.GetLocalizedText("HUD_Dungeon");
    }

    private void RefreshCurrency()
    {
        string gold = NumberFormatter.FormatNumber(GameManager.Instance.stats.GetCurrency(PlayerProgressType.Gold)).ToString();
        string diamond = GameManager.Instance.stats.GetCurrency(PlayerProgressType.Diamond).ToString("N0");

        stageGoldText.text = gold;
        stageDiamondText.text = diamond;
        shopGoldText.text = gold;
        shopDiamondText.text = diamond;
    }
}


