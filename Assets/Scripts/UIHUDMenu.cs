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
    [SerializeField] private Button menuButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI shopText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI menuText;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI stageGoldText;
    [SerializeField] private TextMeshProUGUI stageDiamondText;
    [SerializeField] private TextMeshProUGUI shopGoldText;
    [SerializeField] private TextMeshProUGUI shopDiamondText;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += RefreshLanguage;
        GameManager.Instance.stats.OnCurrencyChanged += RefreshCurrency;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= RefreshLanguage;
        GameManager.Instance.stats.OnCurrencyChanged -= RefreshCurrency;
    }

    private void Start()
    {
        shopButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIShopPage>());
        statButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIGoldUpgradePage>());
        skillButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UISkillPage>());
        inventoryButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIInventoryPage>());
        //menuButton.onClick.AddListener(() => UIManager.Instance.PageOpen<>());

        //-----------------------------------------------------------------------------------------

        RefreshCurrency();
        RefreshLanguage();
    }

    public void RefreshLanguage()
    {
        //shopText.text = DataManager.Instance.HudNames[HUDType.Shop].GetLocalizedText();
        //statText.text = DataManager.Instance.HudNames[HUDType.Stat].GetLocalizedText();
        //skillText.text = DataManager.Instance.HudNames[HUDType.Skill].GetLocalizedText();
        //inventoryText.text = DataManager.Instance.HudNames[HUDType.Inventory].GetLocalizedText(); 
        //menuText.text = DataManager.Instance.HudNames[HUDType.Menu].GetLocalizedText();
        shopText.text = DataManager.Instance.GetLocalizedText("HUD_Shop");
        statText.text = DataManager.Instance.GetLocalizedText("HUD_Stat");
        skillText.text = DataManager.Instance.GetLocalizedText("HUD_Skill");
        inventoryText.text = DataManager.Instance.GetLocalizedText("HUD_Inventory");
        menuText.text = DataManager.Instance.GetLocalizedText("HUD_Menu");
    }

    private void RefreshCurrency()
    {
        string gold = GameManager.Instance.stats.GetGold().ToString();
        string diamond = GameManager.Instance.stats.GetDiamond().ToString();

        stageGoldText.text = gold;
        stageDiamondText.text = diamond;
        shopGoldText.text = gold;
        shopDiamondText.text = diamond;
    }
}


