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
            Debug.LogWarning($"[InventoryManager] ���� ������ ���� �õ���, {itemId}");
            return false;
        }

        ItemType type = item.Data.ItemType;

        //�ش� ������ ������ ���������� Ȯ��
        if (equippedItems.TryGetValue(type, out InventoryItem alreadyEquipped))
        {
            //�̹� �������� �������� �ִٸ� ��������
            alreadyEquipped.IsEquipped = false;
        }

        item.IsEquipped = true;
        equippedItems[type] = item;
        return true;
    }

    public void UnequipItem(ItemType type)
    {
        //�������� ���������� Ȯ���ϰ�, �������̶�� ���� �ʱ�ȭ �� ��ųʸ����� ����
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
        Debug.Log($"������ ������ : {inventory.Count},{inventory.Values.Count}");

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

        Debug.Log("�� �κ��丮");
        Debug.Log(saveData);
        Debug.Log(saveData == null ? "���̺� ������ ����" : "���̺굥���� �������");
        Debug.Log(saveData.inventoryEntries == null ? "�κ���Ʈ�� Ŭ���� ����" : "�κ���Ʈ�� Ŭ���� ����");
        Debug.Log("�� �κ��丮" + saveData.inventoryEntries.Count);
        inventory.Clear();

        foreach (InventoryEntry entry in saveData.inventoryEntries)
        {

            //itemDataTable�� id�� �ش��ϴ� ������ ��������
            ItemData data = DataManager.Instance.GetItemData()[entry.Id];

            //������ �����ͷ� �ʱ�ȭ
            InventoryItem item = new InventoryItem(data, entry.IsUnlocked)
            {
                Level = entry.Level,
                Count = entry.Count,
                IsEquipped = entry.IsEquipped,
            };

            //��ųʸ� �ʱ�ȭ
            inventory[entry.Id] = item;

            Debug.Log($"�κ��丮 ���� : {entry.Id} / {inventory[entry.Id]}");
        }



        //�������� ������ ���� ����
        equippedItems.Clear();

        //������ �о�� �����ͷ� ��ü��
        foreach (InventoryItem item in inventory.Values)
        {
            if (item.IsEquipped)
            {
                equippedItems[item.Data.ItemType] = item;
            }
        }
    }
}
