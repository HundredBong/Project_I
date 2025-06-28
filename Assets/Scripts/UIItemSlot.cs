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
    [Header("�ʱ�ȭ")]
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
        //��� ������ ���׷��̵� �� �� �ֵ��� �˾� ����ֱ�
        //UIManager.Instance.PopupOpen<UIItemUpgradePopup>().Init(itemData);

        //�ϴ� �̰ɷ� �˾��� ���°� Ȯ���̿���
        //�ƴ� �����غ��ϱ� �ܺο��� �̰� ���׷��̵��ص� �ݹ����� Refresh������ ������� �ָ� �����ʳ�?
        //�׷��ϱ� Init�� ����Ǹ� ItemData�� InventoryItem�� �ʱ�ȭ �ƴٴ� ���̴ϱ� �� �� �˾��� ���ڷ� �ѱ�,
        //ItemData���� ���Ŀ� UpgradePrice �־��ְ�
        //�˾����� ���׷��̵��ϸ� UIItemSlot�� Refresh()�� ȣ���ϵ��� �ݹ��� �Ѱ��ָ� ���� ����?

        //??
        //�� �ݹ��� �� ���� �����?
        //OnClick�� �ݹ鵵 ���ڷ� �ѱ�� �ǳ�?
        //�׸��� ���׷��̵� ������ Refresh�� ������� �޶��ϰ�?
        //�׷��ϱ� OnClick���� �˾� ���� �� �ʱ�ȭ
        //�ش� �˾����� ���׷��̵��ϸ� �����۽��� ���ΰ�ħ?


        //�ܺο��� ���Ŀ� InventoryManager.AddItem�� ȣ���ϰ� �Ǹ�, ������� ���� ���� ������
        //�׷� �ű⼭�� �̹� ������ �׷� ���Ե鿡 ���� ó���� �����������
        //���� ��� �̹� ���� �������̸� Count����, 
        //ó�� ��� ���� lockedImage��Ȱ��ȭ ����,

        //AddItem�Ҷ� DataManager�� GetItem�� ȣ���ؼ� ItemData�� ��ȯ�ް� �װɷ� AddItem�� ���ٵ�
        //GetItem�� ���ڴ� �� int���̰�, 
        //�׷� ���� ������Id�� 101~110, 201~210 �̷� ���ε�
        //Random.Range�δ� �ȵɰŰ�,
        //�Դٰ� �̰� ���� Ȯ���� �޶���ϰ�
        //�׷� �������°� Id�� ������ GetItemData�� �� ���� �޾ƿ�
        //�׷� Id�� ��Ƴ��� ����Ʈ�� �ְ�, List<int>
        //�׷� �� ����Ʈ���� Random.Range(List[0], List[List.Count + 1])
        //�䷸�� �̴´ٰ� �ĵ�,
        //Ȯ���� �ٸ�����
        //�׷� ������ �̱� Ȯ�� ������ ���̺� �ۼ��ؾ��ϳ�

        //�ϳ� ��, �� �������� ����� �� PlayerStats���� ������ ���ľ���
        //PlayerStats�� �ϴ� GameManager���� ���������� ������ �� �� �ִµ�
        //��� ������ ������ ������ �˰�,
        //��? �̰� InventoryManager�� ���� �����ʳ�?
        //�ƹ�ư ��� ������ ��������, �� ������ ��������.
        //�̰� ����ȿ�� ��°͵� �ִٺ��ϱ�
        //��⸸ �ص� ���Ŀ� �����ؾ��ϰ�,
        //�� �����ϸ� �߰����� ���ĵ� �����ؾ��ϰ�
        //��
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