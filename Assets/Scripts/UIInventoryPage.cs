using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class UIInventoryPage : UIPage
{
    [Header("생성할 프리팹, 루트")]
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Transform contentRoot;

    [Header("필터 버튼")]
    [SerializeField] private Button showWeaponButton;
    [SerializeField] private Button showArmorButton;
    [SerializeField] private Button showNecklaceButton;

    [Header("버튼 텍스트")]
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
        //UIManager의 Awake에서 전부 SetActive(false)시켜주긴 하는데
        //그럼 유니태스크 안쓰고 OnEnable로 해도 되지 않나
        //그래도 가끔 Awake보다 OnEnable이 먼저 실행될 때도 있으니 안전하게 유니태스크 사용
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
        //아이템 팝업 떠있으면 지워주기
        UIManager.Instance.PopupClose();
        RefreshAll();
    }

    private void Refresh()
    {
        weaponButtonText.text = DataManager.Instance.GetLocalizedText("UI_Weapon");
        armorButtonText.text = DataManager.Instance.GetLocalizedText("UI_Armor");
        necklaceButtonText.text = DataManager.Instance.GetLocalizedText("UI_Necklace");
    }

    [ContextMenu("테스트")]
    public void RefreshAll()
    {
        foreach (var item in itemSlots)
        {
            item.Value.Refresh();
        }
    }
}
