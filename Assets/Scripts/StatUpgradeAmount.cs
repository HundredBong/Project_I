using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatUpgradeAmount
{
    public static int statSlotAmount = LocalSetting.LoadUpgradeAmount();

    private static List<StatUpgradeAmountSelector> selectors = new List<StatUpgradeAmountSelector>();

    //인터페이스 기반 구조가 더 좋지만, amount는 스탯, 업그레이드에서만 사용하므로 직접 참조로 간단하게 구현함
    private static List<UIGoldUpgradeSlot> goldUpgradeSlots = new List<UIGoldUpgradeSlot>(8);

    public static void Register(StatUpgradeAmountSelector selector)
    {
        if (selectors.Contains(selector) == false)
        {
            selectors.Add(selector);
        }
    }

    public static void Register(UIGoldUpgradeSlot slot)
    {
        if (goldUpgradeSlots.Contains(slot) == false)
        {
            goldUpgradeSlots.Add(slot);
        }
    }

    public static void Unregister(StatUpgradeAmountSelector selector)
    {
        selectors.Remove(selector);
    }

    public static void Unregister(UIGoldUpgradeSlot slot)
    {
        goldUpgradeSlots.Remove(slot);
    }

    public static void NotifyChange()
    {
        foreach (var button in selectors)
        {
            button.UpdateColor();
        }

        foreach (UIGoldUpgradeSlot slot in goldUpgradeSlots)
        {
            slot.Refresh();
        }
    }

    public static void Clear()
    {
        selectors.Clear();
        goldUpgradeSlots.Clear();
    }
}
