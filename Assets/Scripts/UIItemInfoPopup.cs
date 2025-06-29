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

    private Action onUpgraded;
    private ItemData itemData;
    private InventoryItem inventoryItem;

    public void Init(ItemData itemData, InventoryItem inventoryItem, Action onUpgraded)
    {
        this.itemData = itemData;
        this.inventoryItem = inventoryItem;
        this.onUpgraded = onUpgraded;

        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        gradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}");
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count}";

        upgradeTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeType");
        upgradeTypeButton.onClick.AddListener(() => { Debug.Log("업그레이드 패널 "); });
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(() => { Debug.Log("합성 패널"); });

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue * inventoryItem.Level}"; //TODO : 식 확인 필요

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{(int)GameManager.Instance.stats.GetProgress(PlayerProgressType.EnhanceStone)} / {itemData.UpgradePrice}";

        upgradeButton.onClick.RemoveListener(OnClickEnhance); //기존 이벤트 제거 안하면 중복으로 강화됨
        upgradeButton.onClick.AddListener(OnClickEnhance);

        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButton.onClick.AddListener(() => { Debug.Log("기존 장비 해제 및 새로운 장비 장착"); });
        equipButtonText.text = DataManager.Instance.GetLocalizedText("UI_Equip");
    }

    private void Refresh()
    {
        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        gradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}");
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count})";

        upgradeTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeType");
        upgradeTypeButton.onClick.AddListener(() => { Debug.Log("업그레이드 패널 "); });
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(() => { Debug.Log("합성 패널"); });

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue * inventoryItem.Level}"; //TODO : 식 확인 필요

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{itemData.UpgradePrice}"; //TODO : 플레이어 재화 보유량 표시
        upgradeButton.onClick.AddListener(() => { Debug.Log("업그레이드 됨"); });
        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButton.onClick.AddListener(() => { Debug.Log("기존 장비 해제 및 새로운 장비 장착"); });
        equipButtonText.text = DataManager.Instance.GetLocalizedText("UI_Equip");
    }

    public void OnClickEnhance()
    {
        //PlayerStats에서 강화석으로 강화 시도
        bool success = GameManager.Instance.stats.TryEnhanceItem(itemData, inventoryItem);

        if (success == true)
        {
            onUpgraded?.Invoke(); //UIItemSlot의 UI 갱신
            Refresh(); //자기 자신의 UI 갱신
        }
    }

}
