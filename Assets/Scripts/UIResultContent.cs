using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResultContent : PooledUI
{
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private Image icon;
    [SerializeField] private Image _shineImage;

    public void Initialize(ItemData data)
    {
        DontDestroyOnLoad(gameObject);

        gradeText.text = $"{data.Stage}{DataManager.Instance.GetLocalizedText("UI_Grade")}";
        icon.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);

        //에픽등급 이상이라면
        if (GradeType.Epic <= data.GradeType)
        {
            UITweening.PlayShine(_shineImage);
        }
    }

    public void Initialize(SkillData data)
    {
        gradeText.text = "";
        icon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);

        if (GradeType.Epic <= data.Grade)
        {
            UITweening.PlayShine(_shineImage);
        }
    }
}
