using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIRewardSlot : PooledUI
{
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private Image rewardIcon;

    public void Init(string spriteKey, int amount)
    {
        _amountText.text = amount.ToString("N0");
        rewardIcon.sprite = DataManager.Instance.GetSpriteByKey(spriteKey);
    }
}
