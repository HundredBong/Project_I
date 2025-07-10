using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemInfoPopup : UIPopup, IPointerDownHandler, IPointerUpHandler
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
    [SerializeField] private Image tweeningImage;
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
    private Action onSynthesisComplete;
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

        LanguageManager.OnLanguageChanged += Refresh; 
    }

    private void OnDisable()
    {
        minButton.onClick.RemoveListener(OnClickMinButton);
        negativeButton.onClick.RemoveListener(OnClickNegativeButton);
        maxButton.onClick.RemoveListener(OnClickMaxButton);
        positiveButton.onClick.RemoveListener(OnClickPositiveButton);

        LanguageManager.OnLanguageChanged -= Refresh; 
    }

    public void Init(ItemData itemData, InventoryItem inventoryItem, Action onUpgraded, Action onSynthesisComplete)
    {
        this.itemData = itemData;
        this.inventoryItem = inventoryItem;
        this.onUpgraded = onUpgraded;
        this.onSynthesisComplete = onSynthesisComplete;


        upgradeTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeType");
        upgradeTypeButton.onClick.AddListener(ShowUpgradePanel);
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(ShowSynthesisPanel);

        if (showingUpgradePanel)
        {
            ShowUpgradePanel();
        }
        else
        {
            ShowSynthesisPanel();
        }

        //nextItem = InventoryManager.Instance.GetItem(itemData.Id + 1).Data; //현재 ID보다 하나 높은ID를 가진 아이템 불러오기
        //nextItem = DataManager.Instance.GetItemData()[itemData.Id + 1];

        //아이템 테이블에 다음 아이템이 없을경우
        if (DataManager.Instance.GetItemData().TryGetValue(itemData.Id + 1, out nextItem) == false)
        {
            //업그레이드 패널만 보여주고, 합성 패널은 비활성화
            ShowUpgradePanel();
            synthesisButton.interactable = false;
        }

        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        //일반 1단계 라는 식을 표현하려면 {아이템 등급} {n} {단계}
        gradeText.text = $"{DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}")} {itemData.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count}";


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


        synthesisButton.onClick.RemoveListener(OnClickSynthesisButton);
        synthesisButton.onClick.AddListener(OnClickSynthesisButton);

        synthesisButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");

        batchSynthesisButton.onClick.RemoveListener(OnClickBatchSynthesisButton);
        batchSynthesisButton.onClick.AddListener(OnClickBatchSynthesisButton);

        batchSynthesisButtonText.text = DataManager.Instance.GetLocalizedText("UI_BatchSynthesis");

        //초기에 최대로 보이게 설정
        synthesisCount = Mathf.Min(synthesisCount, inventoryItem.Count / 5);
        synthesisCountText.text = synthesisCount.ToString();

        synthesisButton.interactable = synthesisCount != 0;
        batchSynthesisButton.interactable = synthesisCount != 0;


    }

    private void Refresh()
    {
        itemIconImage.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        gradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{itemData.GradeType.ToString()}");
        itemNameText.text = DataManager.Instance.GetLocalizedText(itemData.NameKey);
        itemLevelText.text = $"Lv. {inventoryItem.Level} / {itemData.MaxLevel}";
        itemCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {inventoryItem.Count}";

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

        synthesisButton.interactable = synthesisCount != 0;
        batchSynthesisButton.interactable = synthesisCount != 0;
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
        Debug.Log($"[UIItemInfoPopup] 아이템 강화 시도 : {success}");
        UIShineEffect.PlayShine(tweeningImage);
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

        synthesisCount = Mathf.Min(synthesisCount + 1, inventoryItem.Count / 5);

        RefreshSynthesisUI();
    }

    private void RefreshSynthesisUI()
    {
        int usedCount = synthesisCount * 5;
        currentItemCount.text = $"{inventoryItem.Count} (-{usedCount})";
        nextItemCount.text = $"+{synthesisCount}";
        synthesisCountText.text = synthesisCount.ToString();

        synthesisButton.interactable = synthesisCount != 0;
        batchSynthesisButton.interactable = synthesisCount != 0;
    }

    private void OnClickSynthesisButton()
    {
        //합성할 횟수 * 5만큼 현재 아이템 갯수 차감
        //합성한 횟수만큼 itemData.Id + 1의 아이템을 Add

        if (InventoryManager.Instance.SubtractItem(itemData, synthesisCount * 5))
        {
            //인벤토리에 아이템 추가 및 저장
            InventoryManager.Instance.AddItem(nextItem, synthesisCount);
            GameManager.Instance.statSaver.RequestSave(InventoryManager.Instance.GetSaveData());
            //onUpgraded?.Invoke(); //개별 아이템 슬롯 초기화

            //UI 새로고침 및 스탯 반영되도록 재계산
            onSynthesisComplete?.Invoke();
            Refresh();
            RefreshSynthesisUI();
            GameManager.Instance.stats.RecalculateStats();

        }

    }

    private void OnClickBatchSynthesisButton()
    {
        //아이템 A가 100개 있음 -> 일괄 합성하면 B가 20개 생성됨 -> B가 5개 이상이니까 합성 또 진행함 -> C가 4개 생성됨 -> 중단

        int currentId = itemData.Id;

        while (true)
        {
            //현재 아이템 정보 받아오기
            InventoryItem currentItem = InventoryManager.Instance.GetItem(currentId);

            //아이템 정보를 받아올 수 없거나, 아이템의 숫자가 5 미만이라면 합성 중지
            if (currentItem == null || currentItem.Count > 5)
            {
                break;
            }

            //다음 아이템이 존재하지 않는다면 중지
            if (DataManager.Instance.GetItemData().TryGetValue(itemData.Id + 1, out ItemData nextItemData) == false)
            {
                break; 
            }

            //현재 아이템을 합성할 횟수 * 5만큼 차감
            InventoryManager.Instance.SubtractItem(currentItem.Data, synthesisCount * 5);
            //합성할 횟수만큼 다음 아이템 추가
            InventoryManager.Instance.AddItem(nextItemData, synthesisCount);

            currentId++;
        }

        GameManager.Instance.statSaver.RequestSave(InventoryManager.Instance.GetSaveData());

        onSynthesisComplete?.Invoke();
        Refresh();
        RefreshSynthesisUI();
        GameManager.Instance.stats.RecalculateStats();
    }

    public void OnPointerDown(PointerEventData data)
    {
        InvokeRepeating(nameof(Test2), 0f, 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke(nameof(Test2));
    }

    private void Test2()
    {
        Debug.Log("Test2 called");
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
