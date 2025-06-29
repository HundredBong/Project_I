using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfoPopup : UIPopup
{
    //등급 n단계 tmp
    //아이템 이름 tmp
    //아이템 레벨 / 맥스 레벨 string $"{float}"
    //보유 수량 string $"{int}"

    //장착 효과 tmp
    //장착 효과 종류 tmp
    //장착 효과 value tmp

    //보유 효과 tmp
    //종류, value

    //강화 재료 텍스트
    //강화 재료 이미지
    //강화 재료 float

    //강화 버튼
    //합성 -> 이거 다음 단계 아이콘 보여줘야 함 
    //1. CSV의 ItemData에 다은 단게 아이콘 키, Grade, Stage 추가하기?
    //GetItemData해서 자기 데이터 Id + 1 의 데이터를 가져오는게 더 좋지 않나
    //오와우~

    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    [Space(20)]
    [SerializeField] private Button upgradeTypeButton;
    [SerializeField] private TextMeshProUGUI upgradeTypeButtonText;
    [SerializeField] private Button synthesisTypeButton;
    [SerializeField] private TextMeshProUGUI synthesisTypeButtonText;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI effectTypeText;
    [SerializeField] private TextMeshProUGUI effectValueText;

    [SerializeField] private TextMeshProUGUI ownedEffectText;
    [SerializeField] private TextMeshProUGUI ownedTypeText;
    [SerializeField] private TextMeshProUGUI ownedValueText;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI upgradeMaterialText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI equipButtonText;

    [Space(20)]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject synthesisPanel;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI currentItemGradeText;
    [SerializeField] private Image currentItemImage;
    [SerializeField] private TextMeshProUGUI nextItemGradeText;
    [SerializeField] private Image nextItemImage;
    [SerializeField] private TextMeshProUGUI currentItemCount;
    [SerializeField] private TextMeshProUGUI nextItemCount;
    [SerializeField] private TextMeshProUGUI synthesisDesc;
    [SerializeField] private TextMeshProUGUI synthesisCountText;
    [SerializeField] private Button synthesisButton;
    [SerializeField] private TextMeshProUGUI synthesisButtonText;
    [SerializeField] private Button batchSynthesisButton;
    [SerializeField] private TextMeshProUGUI batchSynthesisButtonText;


    [Space(20)]
    [SerializeField] private Button minButton;
    [SerializeField] private Button negativeButton;
    [SerializeField] private Button maxButton;
    [SerializeField] private Button positiveButton;

    private Action onUpgraded;
    private ItemData itemData;
    private InventoryItem inventoryItem;
    private int synthesisCount = 0;
    private bool showingUpgradePanel = true;
    private ItemData nextItem = null;

    private void OnEnable()
    {
        minButton.onClick.AddListener(OnClickMinButton);
        negativeButton.onClick.AddListener(OnClickNegativeButton);
        maxButton.onClick.AddListener(OnClickMaxButton);
        positiveButton.onClick.AddListener(OnClickPositiveButton);
    }

    public void Init(ItemData itemData, InventoryItem inventoryItem, Action onUpgraded)
    {
        this.itemData = itemData;
        this.inventoryItem = inventoryItem;
        this.onUpgraded = onUpgraded;

        //nextItem = InventoryManager.Instance.GetItem(itemData.Id + 1).Data; //현재 ID보다 하나 높은ID를 가진 아이템 불러오기
        nextItem = DataManager.Instance.GetItemData()[itemData.Id + 1];

        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        //일반 1단계 라는 식을 표현하려면 {아이템 등급} {n} {단계}
        gradeText.text = $"{DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}")} {itemData.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count}";

        upgradeTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeType");
        upgradeTypeButton.onClick.AddListener(ShowUpgradePanel);
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(ShowSynthesisPanel);

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue + (itemData.BaseValuePerLevel * inventoryItem.Level):F0}";

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue + (itemData.OwnedValuePerLevel * inventoryItem.Level):F0}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{(int)GameManager.Instance.stats.GetProgress(PlayerProgressType.EnhanceStone)} / {itemData.UpgradePrice}";

        upgradeButton.onClick.RemoveListener(OnClickEnhance); //기존 이벤트 제거 안하면 중복으로 강화됨
        upgradeButton.onClick.AddListener(OnClickEnhance);

        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButton.onClick.AddListener(OnClickEquip);
        equipButtonText.text = inventoryItem.IsEquipped == true ? DataManager.Instance.GetLocalizedText("UI_Equipped") : DataManager.Instance.GetLocalizedText("UI_Equip");
        equipButton.interactable = inventoryItem.IsEquipped == true ? false : true;

        currentItemGradeText.text = $"{itemData.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        currentItemImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        nextItemGradeText.text = $"{nextItem.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        nextItemImage.sprite = DataManager.Instance.GetSpriteByKey(nextItem.IconKey);


        int usedCount = synthesisCount * 5;
        currentItemCount.text = $"{inventoryItem.Count} (-{usedCount})";
        nextItemCount.text = $"+{synthesisCount}";

        synthesisDesc.text = DataManager.Instance.GetLocalizedText("UI_ItemSynthesisDesc");

        synthesisCountText.text = synthesisCount.ToString();

        synthesisButton.onClick.RemoveListener(OnClickSynthesisButton);
        synthesisButton.onClick.AddListener(OnClickSynthesisButton);

        synthesisButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");

        batchSynthesisButton.onClick.RemoveListener(OnClickBatchSynthesisButton);
        batchSynthesisButton.onClick.AddListener(OnClickBatchSynthesisButton);

        batchSynthesisButtonText.text = DataManager.Instance.GetLocalizedText("UI_BatchSynthesis");



        if (showingUpgradePanel)
        {
            ShowUpgradePanel();
        }
        else
        {
            ShowSynthesisPanel();
        }
    }

    private void Refresh()
    {
        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        gradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}");
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count})";

        upgradeTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeType");
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue + (itemData.BaseValuePerLevel * inventoryItem.Level):F0}";

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue + (itemData.OwnedValuePerLevel * inventoryItem.Level):F0}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{(int)GameManager.Instance.stats.GetProgress(PlayerProgressType.EnhanceStone)} / {itemData.UpgradePrice}";
        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButtonText.text = inventoryItem.IsEquipped == true ? DataManager.Instance.GetLocalizedText("UI_Equipped") : DataManager.Instance.GetLocalizedText("UI_Equip");
        equipButton.interactable = inventoryItem.IsEquipped == true ? false : true;

    }

    private void OnClickEnhance()
    {
        //PlayerStats에서 강화석으로 강화 시도
        bool success = GameManager.Instance.stats.TryEnhanceItem(itemData, inventoryItem);

        if (success == true)
        {
            onUpgraded?.Invoke(); //UIItemSlot의 UI 갱신
            Refresh(); //자기 자신의 UI 갱신
            GameManager.Instance.stats.RecalculateStats(); //스탯 재계산
        }
    }

    private void OnClickEquip()
    {
        InventoryManager.Instance.UnequipItem(itemData.ItemType);
        InventoryManager.Instance.EquipItem(itemData.Id);
        Refresh();
        GameManager.Instance.stats.RecalculateStats();
    }

    private void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        synthesisPanel.SetActive(false);
        showingUpgradePanel = true;
    }

    private void ShowSynthesisPanel()
    {
        upgradePanel.SetActive(false);
        synthesisPanel.SetActive(true);
        showingUpgradePanel = false;
    }

    private void OnClickMinButton()
    {
        synthesisCount = 0;
        RefreshSynthesisUI();
    }

    private void OnClickNegativeButton()
    {
        synthesisCount = Mathf.Max(0, synthesisCount - 1);
        RefreshSynthesisUI();
    }

    private void OnClickMaxButton()
    {
        if (itemData == null || nextItem == null) { return; }
        synthesisCount = inventoryItem.Count / 5;
        RefreshSynthesisUI();
    }

    private void OnClickPositiveButton()
    {
        if (itemData == null || nextItem == null) { return; }
        synthesisCount = Mathf.Max(synthesisCount + 1, inventoryItem.Count / 5);
        RefreshSynthesisUI();
    }

    private void RefreshSynthesisUI()
    {
        int usedCount = synthesisCount * 5;
        currentItemCount.text = $"{inventoryItem.Count} (-{usedCount})";
        nextItemCount.text = $"+{synthesisCount}";
        synthesisCountText.text = synthesisCount.ToString();
    }

    private void OnClickSynthesisButton()
    {
        //합성할 횟수 * 5만큼 현재 아이템 갯수 차감
        //합성한 횟수만큼 itemData.Id + 1의 아이템을 Add

        InventoryManager.Instance.SubtractItem(itemData, synthesisCount * 5);
        InventoryManager.Instance.AddItem(nextItem, synthesisCount);

        onUpgraded?.Invoke();
        Refresh();
        RefreshSynthesisUI();
        GameManager.Instance.stats.RecalculateStats();
    }

    private void OnClickBatchSynthesisButton()
    {

    }

    [ContextMenu("테스트")]
    private void Test()
    {
        InventoryManager.Instance.AddItem(itemData, 10);


        onUpgraded?.Invoke();
        Refresh();
        RefreshSynthesisUI();
        GameManager.Instance.stats.RecalculateStats();
    }
}
