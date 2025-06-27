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
        if (inventory.TryGetValue(itemId, out InventoryItem item))
        {
            return item;
        }
        return null;
    }

    public bool HasItem(int itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    public void AddItem(ItemData data, int count = 1)
    {
        if (inventory.TryGetValue(data.Id, out InventoryItem item))
        {
            item.Count += count;
            item.IsUnlocked = true;
        }
        else
        {
            inventory[data.Id] = new InventoryItem(data, true)
            {
                Count = count
            };
        }
    }

    public bool EquipItem(int itemId)
    {
        if (inventory.TryGetValue(itemId, out InventoryItem item) == false)
        {
            Debug.LogWarning($"[InventoryManager] 없는 아이템 작착 시도함, {itemId}");
            return false;
        }

        ItemType type = item.Data.ItemType;

        //해당 부위에 아이템 착용중인지 확인
        if (equippedItems.TryGetValue(type, out InventoryItem alreadyEquipped))
        {
            //이미 착용중인 아이템이 있다면 장착해제
            alreadyEquipped.IsEquipped = false;
        }

        item.IsEquipped = true;
        equippedItems[type] = item;
        return true;
    }

    public void UnequipItem(ItemType type)
    {
        //아이템을 장착중인지 확인하고, 장착중이라면 변수 초기화 및 딕셔너리에서 제거
        if (equippedItems.TryGetValue(type, out InventoryItem item))
        {
            item.IsEquipped = false;
            equippedItems.Remove(type);
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

            Debug.Log($"인벤토리 정보 : {entry.Id} / {inventory[entry.Id]}");
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
    }
}
