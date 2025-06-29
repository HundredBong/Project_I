using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfoPopup : UIPopup
{
    //��� n�ܰ� tmp
    //������ �̸� tmp
    //������ ���� / �ƽ� ���� string $"{float}"
    //���� ���� string $"{int}"

    //���� ȿ�� tmp
    //���� ȿ�� ���� tmp
    //���� ȿ�� value tmp

    //���� ȿ�� tmp
    //����, value

    //��ȭ ��� �ؽ�Ʈ
    //��ȭ ��� �̹���
    //��ȭ ��� float

    //��ȭ ��ư
    //�ռ� -> �̰� ���� �ܰ� ������ ������� �� 
    //1. CSV�� ItemData�� ���� �ܰ� ������ Ű, Grade, Stage �߰��ϱ�?
    //GetItemData�ؼ� �ڱ� ������ Id + 1 �� �����͸� �������°� �� ���� �ʳ�
    //���Ϳ�~

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
        upgradeTypeButton.onClick.AddListener(() => { Debug.Log("���׷��̵� �г� "); });
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(() => { Debug.Log("�ռ� �г�"); });

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue * inventoryItem.Level}"; //TODO : �� Ȯ�� �ʿ�

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{(int)GameManager.Instance.stats.GetProgress(PlayerProgressType.EnhanceStone)} / {itemData.UpgradePrice}";

        upgradeButton.onClick.RemoveListener(OnClickEnhance); //���� �̺�Ʈ ���� ���ϸ� �ߺ����� ��ȭ��
        upgradeButton.onClick.AddListener(OnClickEnhance);

        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButton.onClick.AddListener(() => { Debug.Log("���� ��� ���� �� ���ο� ��� ����"); });
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
        upgradeTypeButton.onClick.AddListener(() => { Debug.Log("���׷��̵� �г� "); });
        synthesisTypeButtonText.text = DataManager.Instance.GetLocalizedText("UI_Synthesis");
        synthesisTypeButton.onClick.AddListener(() => { Debug.Log("�ռ� �г�"); });

        effectText.text = DataManager.Instance.GetLocalizedText("UI_EquippedEffect");
        effectTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.EquippedEffectType}");
        effectValueText.text = $"{itemData.BaseValue * inventoryItem.Level}"; //TODO : �� Ȯ�� �ʿ�

        ownedEffectText.text = DataManager.Instance.GetLocalizedText("UI_OwnedEffect");
        ownedTypeText.text = DataManager.Instance.GetLocalizedText($"Item_Effect_{itemData.OwnedEffectType}");
        ownedValueText.text = $"{itemData.OwnedValue}";

        upgradeMaterialText.text = DataManager.Instance.GetLocalizedText("UI_UpgradeMaterial");
        upgradePriceText.text = $"{itemData.UpgradePrice}"; //TODO : �÷��̾� ��ȭ ������ ǥ��
        upgradeButton.onClick.AddListener(() => { Debug.Log("���׷��̵� ��"); });
        upgradeButtonText.text = DataManager.Instance.GetLocalizedText("UI_LevelUp");
        equipButton.onClick.AddListener(() => { Debug.Log("���� ��� ���� �� ���ο� ��� ����"); });
        equipButtonText.text = DataManager.Instance.GetLocalizedText("UI_Equip");
    }

    public void OnClickEnhance()
    {
        //PlayerStats���� ��ȭ������ ��ȭ �õ�
        bool success = GameManager.Instance.stats.TryEnhanceItem(itemData, inventoryItem);

        if (success == true)
        {
            onUpgraded?.Invoke(); //UIItemSlot�� UI ����
            Refresh(); //�ڱ� �ڽ��� UI ����
        }
    }

}
