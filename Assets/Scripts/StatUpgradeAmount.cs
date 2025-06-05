using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatUpgradeAmount
{
    public static int statSlotAmount = 1;

    private static List<StatUpgradeAmountSelector> selectors = new List<StatUpgradeAmountSelector>();

    public static void Register(StatUpgradeAmountSelector selector)
    {
        if (selectors.Contains(selector) == false)
        {
            selectors.Add(selector);
        }
    }

    public static void NotifyChange()
    {
        foreach (var button in selectors)
        {
            button.UpdateColor();
        }
    }

    public static void Clear()
    {
        selectors.Clear();
    }
}
