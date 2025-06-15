using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    //SKillPage로 생성될 프리팹
    [SerializeField] private TextMeshProUGUI skillNameText; //스킬 이름
    [SerializeField] private TextMeshProUGUI currentLevelText; //현재 레벨
    [SerializeField] private TextMeshProUGUI currentCount; //현재 스킬 보유 수량
    [SerializeField] private TextMeshProUGUI requiredCount; //각성에 필요한 스킬 보유 수량
    [SerializeField] private Button selectButton;
    [SerializeField] private Image skillIcon;

    private SkillData skillData;

    //SkillPage의 Start에서 SkillId대로 데이터를 넘겨받음
    public void Init(SkillData data)
    {
        skillData = data;
        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);

        selectButton.onClick.AddListener(OnClick);
    }

    public void Refresh()
    {
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
    }

    private void OnClick()
    {
        //var popup = UIManager.Instance.PopupOpen<UISkillInfoPopup>();
        //popup.SetSkill(data);sk
    }
}
