using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class UIInventoryPage : UIPage
{
    [Header("������ ������, ��Ʈ")]
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("���� ��ư")]
    [SerializeField] private Button showWeaponButton;
    [SerializeField] private Button showArmorButton;
    [SerializeField] private Button showNecklaceButton;

    private Dictionary<int, UIItemSlot> itemSlots = new Dictionary<int, UIItemSlot>();
    private ItemType currentItemType;

    private async void Start()
    {
        Debug.Log("�κ� ��ŸƮ1");
        //UIManager�� Awake���� ���� SetActive(false)�����ֱ� �ϴµ�
        //�׷� �����½�ũ �Ⱦ��� OnEnable�� �ص� ���� �ʳ�
        //�׷��� ���� Awake���� OnEnable�� ���� ����� ���� ������ �����ϰ� �����½�ũ ���
        await UniTask.WaitUntil(() => GameManager.Instance.inventoryReady);
        Debug.Log("�κ� ��ŸƮ2");

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

    [ContextMenu("�׽�Ʈ")]
    public void RefreshAll()
    {
        foreach (var item in itemSlots)
        {
            Debug.Log("���ΰ�ħ : " + itemSlots.Values.Count);
            item.Value.Refresh();
        }
    }
}
