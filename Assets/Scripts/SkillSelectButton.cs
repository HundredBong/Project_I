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

    private SkillData skillData; //이 버튼이 담당하는 스킬

    public void SetSkill(SkillData skillData, Action<SkillData> onClick)
    {
        Button button = GetComponentInChildren<Button>();

        //어떤 스킬인지 기억하고, 아이콘 교체하고 전달반은 콜백을 실행함.
        //버튼은 클릭될 때 뭔가를 해야하는데 그걸 외부에서 정의할 수 있도록 함.
        this.skillData = skillData;
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.spriteDic[skillData.SkillIcon];

        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);

        button.onClick.AddListener(() =>
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
}
