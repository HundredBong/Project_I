using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResultContent : MonoBehaviour, IPooledObject
{
    public GameObject prefabReference { get; set; }

    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private Image icon;

    public void Initialize(ItemData data)
    {
        gradeText.text = $"{data.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        icon.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
    }

    public void Initialize(SkillData data)
    {
        gradeText.text = "";
        icon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);
    }
}
