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

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += RefreshLanguage;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= RefreshLanguage;
    }

    private void Start()
    {
        //shopButton.onClick.AddListener(() => UIManager.Instance.PageOpen<>());
        statButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIStatPage>());
       skillButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UISkillPage>());
        //inventoryButton.onClick.AddListener(() => UIManager.Instance.PageOpen<>());
        //menuButton.onClick.AddListener(() => UIManager.Instance.PageOpen<>());

        //-----------------------------------------------------------------------------------------

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
}

    
