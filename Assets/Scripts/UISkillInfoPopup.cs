using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISkillInfoPopup : UIPopup
{
    [Header("�̹���")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image skillStatus;
    [SerializeField] private Image skillAwakenIcon; 

    [Header("�ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI skillGradeText;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillCountText;    
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillEffectName;
    [SerializeField] private TextMeshProUGUI skillEffectDescText;
    [SerializeField] private TextMeshProUGUI skillEffectValueText; 

    [Header("��ư")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button awakenButton;
    [SerializeField] private Button decompositionButton;

    [Header("������Ʈ")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject awakenPanel;
    [SerializeField] private GameObject decompositionPanel;

    public void Init(SkillData data)
    {
        //�̹���
        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
        //skillStatus.sprite = DataManager.Instance.spriteDic[data.StatusEffect.ToString()];
        skillAwakenIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];

        //�ؽ�Ʈ
        skillGradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{data.Grade}");
        skillNameText.text = DataManager.Instance.GetLocalizedText(data.NameKey);
        skillCountText.text = $"{DataManager.Instance.GetLocalizedText("Skill_Count")}"; //TODO: DataManager�� �߰�, ���� �÷��̾� ��ų ������ ���� �ʿ�  
        skillLevelText.text = $"Lv.1/{data.MaxLevel}"; //TODO:���� �÷��̾� ��ų ������ ���� �ʿ�
        skillDescriptionText.text = GetSkillDesc(data, data.SkillId);
        skillEffectName.text = DataManager.Instance.GetLocalizedText("Skill_Effect"); //DataManager�� �߰� 
        skillEffectDescText.text = DataManager.Instance.GetLocalizedText($"Skill_Effect_{data.EffectType.ToString()}"); //DataManager�� �߰�
        skillEffectValueText.text = $"{data.PassiveValue}%"; //TODO : ��ų ������ Increase ���� ����

        //��ư
        upgradeButton.onClick.AddListener(ShowUpgradePanel);
        awakenButton.onClick.AddListener(ShowAwakenPanel);
        decompositionButton.onClick.AddListener(ShowDecompositionPanel);

        ShowUpgradePanel();
    }

    private string GetSkillDesc(SkillData data, SkillId id)
    {
        //��� ��ų�� ������ ������ ������ �ƴϴٺ��� �Լ��� ���� ���� �ۼ���
        string rawText = DataManager.Instance.GetLocalizedText(data.DescKey);
        string formattedText = "";
        switch (id)
        {
            case SkillId.Lightning:
                //�ѱ���, ���� ��� ������ �����̶�� �����ϱ�� �ѵ�, ���� ����� �ٸ��ٸ� �� ���� �������� ��.
                //Ȥ�� �𸣴� ����ġ �ͽ��������� �ƴ� �Ϲ� ����ġ������ �ۼ�
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
