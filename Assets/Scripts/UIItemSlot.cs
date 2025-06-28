using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.Timeline.Actions.MenuPriority;


public class UIItemSlot : MonoBehaviour
{
    [Header("초기화")]
    [SerializeField] private TextMeshProUGUI itemStageText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Image itemIcon;

    [Header("UI")]
    [SerializeField] private Image lockedImage;
    [SerializeField] private Button openPopupButton;

    private ItemData itemData;
    private InventoryItem inventoryItem;

    public void Init(ItemData itemData)
    {
        this.itemData = itemData;

        inventoryItem = InventoryManager.Instance.GetItem(itemData.Id);

        if (inventoryItem != null && inventoryItem.IsUnlocked == true)
        {
            lockedImage.gameObject.SetActive(false);
            itemLevelText.text = $"Lv. {inventoryItem.Level}";
            itemStageText.text = $"{itemData.Stage}{DataManager.Instance.GetLocalizedText("Item_Stage")}";
            itemCountText.text = $"{inventoryItem.Count}";
        }
        else
        {
            lockedImage.gameObject.SetActive(true);
            itemLevelText.text = $"";
            itemStageText.text = $"";
            itemCountText.text = $"";
        }

        itemIcon.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
        openPopupButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        //장비 누르면 업그레이드 할 수 있도록 팝업 띄워주기
        //UIManager.Instance.PopupOpen<UIItemUpgradePopup>().Init(itemData);

        //일단 이걸로 팝업을 여는건 확정이에요
        //아니 생각해보니까 외부에서 이걸 업그레이드해도 콜백으로 Refresh같은거 실행시켜 주면 되지않나?
        //그러니까 Init이 실행되면 ItemData와 InventoryItem이 초기화 됐다는 뜻이니까 그 후 팝업에 인자로 넘김,
        //ItemData에는 추후에 UpgradePrice 넣어주고
        //팝업에서 업그레이드하면 UIItemSlot의 Refresh()를 호출하도록 콜백을 넘겨주면 되지 않음?

        //??
        //그 콜백은 또 언제 등록함?
        //OnClick때 콜백도 인자로 넘기면 되나?
        //그리고 업그레이드 끝나면 Refresh를 실행시켜 달라하고?
        //그러니까 OnClick에서 팝업 오픈 및 초기화
        //해당 팝업에서 업그레이드하면 아이템슬롯 새로고침?


        //외부에서 추후에 InventoryManager.AddItem을 호출하게 되면, 예를들어 상점 같은 곳에서
        //그럼 거기서도 이미 생성된 그런 슬롯들에 대한 처리를 해줘야하잖음
        //예를 들면 이미 얻은 아이템이면 Count증가, 
        //처음 얻는 장비면 lockedImage비활성화 까지,

        //AddItem할때 DataManager의 GetItem을 호출해서 ItemData를 반환받고 그걸로 AddItem을 할텐데
        //GetItem의 인자는 또 int형이고, 
        //그럼 지금 아이템Id가 101~110, 201~210 이런 식인데
        //Random.Range로는 안될거고,
        //게다가 이게 전부 확률도 달라야하고
        //그럼 생각나는게 Id는 어차피 GetItemData할 때 전부 받아옴
        //그럼 Id만 모아놓은 리스트에 넣고, List<int>
        //그럼 그 리스트에서 Random.Range(List[0], List[List.Count + 1])
        //요렇게 뽑는다고 쳐도,
        //확률이 다르잖음
        //그럼 아이템 뽑기 확률 데이터 테이블도 작성해야하나

        //하나 더, 이 아이템을 얻었을 때 PlayerStats에도 영향을 끼쳐야함
        //PlayerStats는 일단 GameManager에서 전역적으로 접근은 할 수 있는데
        //어떻게 착용한 아이템 정보를 알고,
        //어? 이거 InventoryManager에 정보 있지않나?
        //아무튼 어떻게 정보를 가져오고, 또 공식을 적용할지.
        //이게 보유효과 라는것도 있다보니까
        //얻기만 해도 공식에 적용해야하고,
        //또 장착하면 추가적인 공식도 적용해야하고
        //어
    }

    public ItemType GetItemType()
    {
        return itemData.ItemType;
    }

    public void Refresh()
    {
        if (inventoryItem != null && inventoryItem.IsUnlocked)
        {
            lockedImage.gameObject.SetActive(false);
            itemLevelText.text = $"Lv. {inventoryItem.Level}";
            itemStageText.text = $"{itemData.Stage}{DataManager.Instance.GetLocalizedText("Item_Stage")}";
            itemCountText.text = $"{inventoryItem.Count}";
        }
        else
        {
            lockedImage.gameObject.SetActive(true);
            itemLevelText.text = "";
            itemStageText.text = "";
            itemCountText.text = "";
        }

        itemIcon.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
    }
}