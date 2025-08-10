using Cysharp.Threading.Tasks;
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
    [SerializeField] private Image skillGemIcon;
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
    [SerializeField] private TextMeshProUGUI showUpgradeButtonText;
    [SerializeField] private TextMeshProUGUI showAwakenButtonText;
    [SerializeField] private TextMeshProUGUI tryUpgradeButtonText;
    [SerializeField] private TextMeshProUGUI tryAwakenButtonText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private TextMeshProUGUI awakenPriceText;

    [Header("버튼")]
    [SerializeField] private Button showUpgradeButton;
    [SerializeField] private Button showAwakenButton;
    [SerializeField] private Button tryUpgradeButton;
    [SerializeField] private Button tryAwakenButton;
    //[SerializeField] private Button decompositionButton;

    [Header("오브젝트")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject awakenPanel;
    //[SerializeField] private GameObject decompositionPanel;

    [Space(20)]
    [SerializeField] private Image[] lockedImageIcons;
    [SerializeField] private Image[] lockImageBackgrounds;
    [SerializeField] private TextMeshProUGUI[] awakenTexts;

    [SerializeField] private Color lockedColor;
    [SerializeField] private Color unlockedColor;

    private SkillData _skillData;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    public void Init(SkillData data)
    {
        _skillData = data;

        //이미지
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);
        //skillStatus.sprite = DataManager.Instance.spriteDic[data.StatusEffect.ToString()];
        skillAwakenIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);

        SetLocalizedText();

        //버튼
        showUpgradeButton.onClick.RemoveAllListeners();
        showUpgradeButton.onClick.AddListener(ShowUpgradePanel);
        showAwakenButton.onClick.RemoveAllListeners();
        showAwakenButton.onClick.AddListener(ShowAwakenPanel);
        skillAwakenIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);
        skillGemIcon.sprite = DataManager.Instance.GetSpriteByKey("UI_SkillGem");
        //decompositionButton.onClick.RemoveAllListeners();
        //decompositionButton.onClick.AddListener(ShowDecompositionPanel);

        tryUpgradeButton.onClick.RemoveAllListeners();
        tryUpgradeButton.onClick.AddListener(OnClickUpgrade);

        tryAwakenButton.onClick.RemoveAllListeners();
        tryAwakenButton.onClick.AddListener(OnClickAwake);
        tryAwakenButton.interactable = SkillManager.Instance.CanAwaken(_skillData);

        ShowUpgradePanel();
    }

    private void SetLocalizedText()
    {
        if (_skillData == null) { return; }

        SkillId skillId = _skillData.SkillId;
        PlayerSkillState state = SkillManager.Instance.GetSkillState(skillId);

        skillGradeText.text = DataManager.Instance.GetLocalizedText($"Grade_{_skillData.Grade}");
        skillNameText.text = DataManager.Instance.GetLocalizedText(_skillData.NameKey);
        skillCountText.text = $"{DataManager.Instance.GetLocalizedText("UI_OwnedCount")} : {SkillManager.Instance.GetSkillState(skillId).OwnedCount}";
        skillLevelText.text = $"Lv. {SkillManager.Instance.GetSkillState(skillId).Level} / {_skillData.MaxLevels[state.AwakenLevel]}";
        skillDescriptionText.text = DataManager.Instance.GetSkillDesc(_skillData, skillId);

        skillEffectName.text = DataManager.Instance.GetLocalizedText("Skill_Effect");
        skillEffectDescText.text = DataManager.Instance.GetLocalizedText($"Skill_Effect_{_skillData.EffectType.ToString()}");

        showUpgradeButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Upgrade")}";
        showAwakenButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Awaken")}";
        tryUpgradeButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Upgrade")}";
        tryAwakenButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Awaken")}";

        int price = GetUpgradePrice(_skillData);

        upgradePriceText.text = $"{GameManager.Instance.stats.SkillGem} / {price}";

        int awakenLevel = state.AwakenLevel;
        int ownedCount = state.OwnedCount;

        if (awakenLevel < _skillData.AwakenRequiredCount.Length)
        {
            int awakenPrice = _skillData.AwakenRequiredCount[awakenLevel];
            awakenPriceText.text = $"{ownedCount} / {awakenPrice}";
        }
        else
        {
            //각성 최대 단계면 MAX 출력
            awakenPriceText.text = DataManager.Instance.GetLocalizedText("UI_Awaken_MAX");
            tryAwakenButton.interactable = false;
        }

        int currentLevel = Mathf.Max(1, state.Level);
        skillEffectValueText.text = $"{_skillData.PassiveValue + (_skillData.PassiveValuePerLevel * currentLevel)}%";

        int count = lockedImageIcons.Length;

        for (int i = 0; i < count; i++)
        {
            bool isUnlocked = i < awakenLevel;

            Debug.Log($"i : {i}, awakenLevel : {awakenLevel}, isUnlocked : {isUnlocked}");

            lockedImageIcons[i].gameObject.SetActive(!isUnlocked);
            lockImageBackgrounds[i].color = isUnlocked ? unlockedColor : lockedColor;
            awakenTexts[i].alignment = isUnlocked ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
            awakenTexts[i].text = isUnlocked ? DataManager.Instance.GetLocalizedText($"Skill_{_skillData.SkillId}_Awaken_{i + 1}") : $"Lv.{i + 1}";
        }
    }


    private void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        awakenPanel.SetActive(false);
        //decompositionPanel.SetActive(false);
    }

    private void ShowAwakenPanel()
    {
        upgradePanel.SetActive(false);
        awakenPanel.SetActive(true);
        //decompositionPanel.SetActive(false);
    }

    //private void ShowDecompositionPanel()
    //{
    //    upgradePanel.SetActive(false);
    //    awakenPanel.SetActive(false);
    //    decompositionPanel.SetActive(true);
    //}

    private int GetUpgradePrice(SkillData data)
    {
        SkillId skillId = data.SkillId;
        PlayerSkillState state = SkillManager.Instance.GetSkillState(data.SkillId);

        int upgradePrice = DataManager.Instance.GetSkill(skillId).UpgradeCost;
        int upgradePricePerLevel = DataManager.Instance.GetSkill(skillId).UpgradeCostPerLevel;
        int currentLevel = Mathf.Max(1, state.Level);
        int final = upgradePrice + (upgradePricePerLevel * currentLevel);

        return final;
    }

    private void OnClickUpgrade()
    {
        int amount = GetUpgradePrice(_skillData);

        if (GameManager.Instance.stats.TrySpendItem(PlayerProgressType.SkillGem, amount))
        {
            if (SkillManager.Instance.TryLevelUp(_skillData))
            {
                Debug.Log("강화에 성공함");       
                SetLocalizedText();
                GameManager.Instance.stats.RecalculateStats();
                GameManager.Instance.statSaver.SavePlayerSkillDataAsync(SkillManager.Instance.BuildSaveData()).Forget();
            }
            else
            {
                Debug.Log("강화에 실패함 1");
                GameManager.Instance.stats.AddCurrency(PlayerProgressType.SkillGem, amount);
            }
        }
        else
        {
            Debug.Log("강화에 실패함 2");
        }
    }

    private void OnClickAwake()
    {
        if (SkillManager.Instance.TryAwaken(_skillData))
        {
            Debug.Log("각성에 성공함");
            SetLocalizedText();
            GameManager.Instance.statSaver.SavePlayerSkillDataAsync(SkillManager.Instance.BuildSaveData()).Forget();
        }
        else
        {
            int requiredCount = SkillManager.Instance.GetRequiredCount(_skillData);
            int currentCount = SkillManager.Instance.GetSkillState(_skillData.SkillId).OwnedCount;

            Debug.LogWarning($"[SkillInfoPopup] 스킬 각성에 실패함, {requiredCount} / {currentCount}");
        }
    }
}
