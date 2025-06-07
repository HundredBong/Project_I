using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUpgradeAmountSelector : MonoBehaviour
{
    public int amountValue;

    public Color unselectedColor;
    public Color selectedColor;

    private Image targetImage;
    private Button button;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        StatUpgradeAmount.Register(this);
        UpdateColor();

        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }

    private void Start()
    {
        if (amountValue == LocalSetting.LoadUpgradeAmount())
        {
            StatUpgradeAmount.statSlotAmount = amountValue;
            StatUpgradeAmount.NotifyChange();
        }
    }

    public void OnClick()
    {
        StatUpgradeAmount.statSlotAmount = amountValue;
        StatUpgradeAmount.NotifyChange();
        LocalSetting.SaveUpgradeAmount(amountValue);
    }

    public void UpdateColor()
    {
        if (StatUpgradeAmount.statSlotAmount == amountValue)
        {
            targetImage.color = selectedColor;
        }
        else
        {
            targetImage.color = unselectedColor;
        }
    }
}
