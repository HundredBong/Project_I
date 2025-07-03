using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<int, InventoryItem> inventory = new Dictionary<int, InventoryItem>();

    private Dictionary<ItemType, InventoryItem> equippedItems = new Dictionary<ItemType, InventoryItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public InventoryItem GetItem(int itemId)
    {
        //인벤토리에 있나 없나 확인
        if (inventory.TryGetValue(itemId, out InventoryItem item))
        {
            return item;
        }
        return null;
    }

    public List<InventoryItem> GetItemList()
    {
        List<InventoryItem> list = new List<InventoryItem>();

        foreach (InventoryItem item in inventory.Values)
        {
            list.Add(item);
        }

        return list;
    }

    public bool HasItem(int itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    public void AddItem(ItemData data, int count = 1)
    {
        Debug.Log($"[InventoryManager] 추가할 아이템 : {data.Id}");

        //아이템이 존재한다면 카운트만 증가시킴
        if (inventory.TryGetValue(data.Id, out InventoryItem inventoryItem))
        {
            //기존 객체의 값만 바꾸는 거라서 문제되지 않음,
            //클래스니까 참조하는 모든 곳에서 동시에 바뀜
            inventoryItem.Count += count;
        }
        //존재하지 않다면 새로 생성
        else
        {
            //새로운 객체로 덮어쓰기함
            //기존에 inventory[data.Id]를 참조하던 UIItemSlot은 옛날 객체를 들고있게 됨
            //딕셔너리에서 접근 불가해짐 -> 딕셔너리의 키는 고유 키임 
            //Remove()한건 아니니까 실제 객체는 메모리에 살아있을 수도 있으나 접근은 불가능 함
            //이 때문에 매번 Refresh에서 최신 객체를 딕셔너리에서 받아와야 딕셔너리와 UI가 동기화 됨
            inventory[data.Id] = new InventoryItem(data, true)
            {
                Count = count
            };
        }

        Debug.Log($"[InventoryManager] 추가된 아이템 : {inventory[data.Id].Data.NameKey}, {inventory[data.Id].IsUnlocked}, {inventory[data.Id].Count}");
    }

    public bool SubtractItem(ItemData data, int count = 1)
    {
        if (inventory.TryGetValue(data.Id, out InventoryItem item))
        {
            if (item.Count - count < 0)
            {
                Debug.LogWarning("[InventoryManager] 가지고있는 아이템보다 많이 차감하려고 함");
                return false;
            }
            item.Count = Mathf.Max(0, item.Count - count);
            return true;
        }
        else
        {
            Debug.LogWarning("[InventoryManager] 없는 아이템 차감 시도함");
            return false;
        }
    }

    public void EquipItem(int itemId)
    {
        if (inventory.TryGetValue(itemId, out InventoryItem item) == false)
        {
            Debug.LogWarning($"[InventoryManager] 없는 아이템 장착 시도함, {itemId}");
            return;
        }

        ItemType type = item.Data.ItemType;

        //해당 부위에 아이템 착용중인지 확인
        if (equippedItems.ContainsKey(type))
        {
            //이미 착용중인 아이템이 있다면 장착해제
            UnequipItem(type);
        }

        item.IsEquipped = true;
        equippedItems[type] = item;

        //장착은 보통 한번씩 이루어지니 바로 저장
        GameManager.Instance.statSaver.SaveInventoryData(GetSaveData());
        //장착하고나면 대미지 재계산
        GameManager.Instance.stats.RecalculateStats();
    }

    public void UnequipItem(ItemType type)
    {
        //아이템을 장착중인지 확인하고, 장착중이라면 변수 초기화 및 딕셔너리에서 제거
        if (equippedItems.TryGetValue(type, out InventoryItem item))
        {
            item.IsEquipped = false;
            equippedItems.Remove(type);
            GameManager.Instance.statSaver.SaveInventoryData(GetSaveData());
            GameManager.Instance.stats.RecalculateStats();
        }
    }

    public InventoryItem GetEquippedItem(ItemType type)
    {
        equippedItems.TryGetValue(type, out InventoryItem item);
        return item;
    }

    public InventorySaveData GetSaveData()
    {
        InventorySaveData data = new InventorySaveData();
        Debug.Log($"저장할 데이터 : {inventory.Count},{inventory.Values.Count}");

        foreach (InventoryItem item in inventory.Values)
        {
            data.inventoryEntries.Add(new InventoryEntry
            {
                Id = item.Data.Id,
                Level = item.Level,
                Count = item.Count,
                IsEquipped = item.IsEquipped,
                IsUnlocked = item.IsUnlocked,
            });
        }

        return data;
    }

    public void SetInventoryData(InventorySaveData saveData)
    {

        Debug.Log("셋 인벤토리");
        Debug.Log(saveData);
        Debug.Log(saveData == null ? "세이브 데이터 널임" : "세이브데이터 살아있음");
        Debug.Log(saveData.inventoryEntries == null ? "인벤엔트리 클래스 널임" : "인벤엔트리 클래스 없음");
        Debug.Log("셋 인벤토리" + saveData.inventoryEntries.Count);
        inventory.Clear();

        foreach (InventoryEntry entry in saveData.inventoryEntries)
        {

            //itemDataTable의 id에 해당하는 아이템 가져오기
            ItemData data = DataManager.Instance.GetItemData()[entry.Id];

            //가져온 데이터로 초기화
            InventoryItem item = new InventoryItem(data, entry.IsUnlocked)
            {
                Level = entry.Level,
                Count = entry.Count,
                IsEquipped = entry.IsEquipped,
            };

            //딕셔너리 초기화
            inventory[entry.Id] = item;

            Debug.Log($"인벤토리 정보 : {entry.Id}");
        }



        //장착중인 아이템 정보 삭제
        equippedItems.Clear();

        //위에서 읽어온 대이터로 대체함
        foreach (InventoryItem item in inventory.Values)
        {
            if (item.IsEquipped)
            {
                equippedItems[item.Data.ItemType] = item;
            }
        }

        GameManager.Instance.stats.RecalculateStats();
    }
}
