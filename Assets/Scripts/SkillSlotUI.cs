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
    [SerializeField] private TextMeshProUGUI requiredCount; //각성에 필요한 스킬 보유 수량
    [SerializeField] private Button selectButton;
    [SerializeField] private Image skillIcon;
    [SerializeField] private GameObject lockOverlay;

    private SkillData skillData;
    private PlayerSkillState skillState;

    //SkillPage의 Start에서 SkillId대로 데이터를 넘겨받음
    public void Init(SkillData data)
    {
        skillData = data;
        skillState = SkillManager.Instance.GetSkillState(data.SkillId);

        Refresh();

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClick);
    }

    public void Refresh()
    {
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.spriteDic[skillData.SkillIcon];

        if(SkillManager.Instance.GetSkillState(skillData.SkillId) != null)
        {
            currentLevelText.text = $"Lv.{skillState.Level}";
            requiredCount.text = $"{skillState.OwnedCount} / {skillData.AwakenRequiredCount[skillState.AwakenLevel]}";
        }

        //스킬 매니저에서 스킬 아이디가 있는지 확인하고, 없다면 해당 스킬은 잠금 상태로 표시
        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
    }

    private void OnClick()
    {
        //잠금 해제된 스킬만 정보 팝업을 띄움
        if (SkillManager.Instance.IsUnlocked(skillData.SkillId) == true)
        {
            UIManager.Instance.PopupOpen<UISkillInfoPopup>().Init(skillData);
        }
        else
        {
            //TODO : string을 인자로 받는 팝업을 UIManager에 추가하여 사용
            Debug.LogWarning($"[SkillSlotUI] 잠겨있는 스킬임, {skillData.SkillId}");
        }
    }
}
