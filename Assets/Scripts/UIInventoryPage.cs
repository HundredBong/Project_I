using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class UIInventoryPage : UIPage
{
    [Header("������ ������, ��Ʈ")]
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("���� ��ư")]
    [SerializeField] private Button showWeaponButton;
    [SerializeField] private Button showArmorButton;
    [SerializeField] private Button showNecklaceButton;

    [Header("��ư �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI weaponButtonText;
    [SerializeField] private TextMeshProUGUI armorButtonText;
    [SerializeField] private TextMeshProUGUI necklaceButtonText;

    private Dictionary<int, UIItemSlot> itemSlots = new Dictionary<int, UIItemSlot>();
    private ItemType currentItemType;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;   
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }

    private async void Start()
    {
        //UIManager�� Awake���� ���� SetActive(false)�����ֱ� �ϴµ�
        //�׷� �����½�ũ �Ⱦ��� OnEnable�� �ص� ���� �ʳ�
        //�׷��� ���� Awake���� OnEnable�� ���� ����� ���� ������ �����ϰ� �����½�ũ ���
        await UniTask.WaitUntil(() => GameManager.Instance.inventoryReady);

        foreach (ItemData itemData in DataManager.Instance.GetItemData().Values)
        {
            GameObject obj = Instantiate(contentPrefab, contentRoot);
            UIItemSlot slot = obj.GetComponent<UIItemSlot>();
            slot.Init(itemData, RefreshAll);
            itemSlots[itemData.Id] = slot;
        }

        FilteringByType(ItemType.Weapon);

        showWeaponButton.onClick.AddListener(() => FilteringByType(ItemType.Weapon));
        showArmorButton.onClick.AddListener(() => FilteringByType(ItemType.Armor));
        showNecklaceButton.onClick.AddListener(() => FilteringByType(ItemType.Necklace));
        Refresh();
    }

    private void FilteringByType(ItemType itemType)
    {
        foreach (UIItemSlot item in itemSlots.Values)
        {
            if (itemType == item.GetItemType())
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }

        currentItemType = itemType;
        //������ �˾� �������� �����ֱ�
        UIManager.Instance.PopupClose();
        RefreshAll();
    }

    private void Refresh()
    {
        weaponButtonText.text = DataManager.Instance.GetLocalizedText("UI_Weapon");
        armorButtonText.text = DataManager.Instance.GetLocalizedText("UI_Armor");
        necklaceButtonText.text = DataManager.Instance.GetLocalizedText("UI_Necklace");
    }

    [ContextMenu("�׽�Ʈ")]
    public void RefreshAll()
    {
        foreach (var item in itemSlots)
        {
            item.Value.Refresh();
        }
    }
}
