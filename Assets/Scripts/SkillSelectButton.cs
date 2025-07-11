using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button selectButton;

    private SkillData skillData;

    public void SetSkill(SkillData skillData, Action<SkillData> onClick)
    {
        this.skillData = skillData;
        //어떤 스킬인지 기억하고, 아이콘 교체하고 전달반은 콜백을 실행함.
        //버튼은 클릭될 때 뭔가를 해야하는데 그걸 외부에서 정의할 수 있도록 함.
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(skillData.SkillIcon);

        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
        selectButton.interactable = SkillManager.Instance.IsUnlocked(skillData.SkillId);

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            //잠금 해제된 스킬만 클릭 이벤트를 실행함.
            if (SkillManager.Instance.IsUnlocked(skillData.SkillId) == true)
            {
                onClick(skillData);
            }
            else
            {
                //TODO : string을 인자로 받는 팝업을 UIManager에 추가하여 사용
                //       GetLocalizedText를 사용
                Debug.LogWarning($"[SkillSelectButton] 잠겨있는 스킬임, {skillData.SkillId}");
            }
        }); 
    }

    public void Refresh()
    {
        //스킬 아이콘과 이름을 갱신하는 메서드
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(skillData.SkillIcon);
        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
        selectButton.interactable = SkillManager.Instance.IsUnlocked(skillData.SkillId);
    }
}
