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
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);
        //skillStatus.sprite = DataManager.Instance.spriteDic[data.StatusEffect.ToString()];
        skillAwakenIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);

        //�ؽ�Ʈ
        skillGradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{data.Grade}");
        skillNameText.text = DataManager.Instance.GetLocalizedText(data.NameKey);
        skillCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {SkillManager.Instance.GetSkillState(data.SkillId).OwnedCount}";
        skillLevelText.text = $"Lv. {SkillManager.Instance.GetSkillState(data.SkillId).Level} / {data.MaxLevel}";
        skillDescriptionText.text = DataManager.Instance.GetSkillDesc(data, data.SkillId);

        skillEffectName.text = DataManager.Instance.GetLocalizedText("Skill_Effect");
        skillEffectDescText.text = DataManager.Instance.GetLocalizedText($"Skill_Effect_{data.EffectType.ToString()}");
        skillEffectValueText.text = $"{data.PassiveValue}%"; //TODO : ��ų ������ Increase ���� ����, �ε� �켱���� ����, ���� : �����̶�

        //��ư
        upgradeButton.onClick.AddListener(ShowUpgradePanel);
        awakenButton.onClick.AddListener(ShowAwakenPanel);
        decompositionButton.onClick.AddListener(ShowDecompositionPanel);

        ShowUpgradePanel();
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
