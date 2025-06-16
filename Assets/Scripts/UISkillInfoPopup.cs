using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISkillInfoPopup : UIPopup
{
    [Header("이미지")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image skillStatus;
    [SerializeField] private Image skillAwakenIcon; 

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI skillGradeText;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillCountText;    
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillEffectName;
    [SerializeField] private TextMeshProUGUI skillEffectDescText;
    [SerializeField] private TextMeshProUGUI skillEffectValueText; 

    [Header("버튼")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button awakenButton;
    [SerializeField] private Button decompositionButton;

    [Header("오브젝트")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject awakenPanel;
    [SerializeField] private GameObject decompositionPanel;

    public void Init(SkillData data)
    {
        //이미지
        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
        //skillStatus.sprite = DataManager.Instance.spriteDic[data.StatusEffect.ToString()];
        skillAwakenIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];

        //텍스트
        skillGradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{data.Grade}");
        skillNameText.text = DataManager.Instance.GetLocalizedText(data.NameKey);
        skillCountText.text = $"{DataManager.Instance.GetLocalizedText("Skill_Count")}"; //TODO: DataManager에 추가, 추후 플레이어 스킬 개수로 변경 필요  
        skillLevelText.text = $"Lv.1/{data.MaxLevel}"; //TODO:추후 플레이어 스킬 레벨로 변경 필요
        skillDescriptionText.text = GetSkillDesc(data, data.SkillId);
        skillEffectName.text = DataManager.Instance.GetLocalizedText("Skill_Effect"); //DataManager에 추가 
        skillEffectDescText.text = DataManager.Instance.GetLocalizedText($"Skill_Effect_{data.EffectType.ToString()}"); //DataManager에 추가
        skillEffectValueText.text = $"{data.PassiveValue}%"; //TODO : 스킬 레벨별 Increase 공식 적용

        //버튼
        upgradeButton.onClick.AddListener(ShowUpgradePanel);
        awakenButton.onClick.AddListener(ShowAwakenPanel);
        decompositionButton.onClick.AddListener(ShowDecompositionPanel);

        ShowUpgradePanel();
    }

    private string GetSkillDesc(SkillData data, SkillId id)
    {
        //모든 스킬이 동일한 형식을 가진게 아니다보니 함수로 빼서 따로 작성함
        string rawText = DataManager.Instance.GetLocalizedText(data.DescKey);
        string formattedText = "";
        switch (id)
        {
            case SkillId.Lightning:
                //한국어, 영어 모두 동일한 포맷이라면 가능하기는 한데, 포맷 방식이 다르다면 다 따로 만들어줘야 함.
                //혹시 모르니 스위치 익스프레션이 아닌 일반 스위치문으로 작성
                formattedText = string.Format(rawText, data.BaseValue, data.HitCount, data.StatucChance, data.Cooldown);
                return formattedText;
            default:
                return rawText;
        }
    }

    private void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        awakenPanel.SetActive(false);
        decompositionPanel.SetActive(false);
    }   

    private void ShowAwakenPanel()
    {
        upgradePanel.SetActive(false);
        awakenPanel.SetActive(true);
        decompositionPanel.SetActive(false);
    }

    private void ShowDecompositionPanel()
    {
        upgradePanel.SetActive(false);
        awakenPanel.SetActive(false);
        decompositionPanel.SetActive(true);
    }
}
