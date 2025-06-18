using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActiveSkillSlot : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Button skillButton;

    private SkillBase equippedSkill;

    public void Init(SkillId id)
    {
        if (id == SkillId.None)
        {
            gameObject.SetActive(false);
            return;
        }
         gameObject.SetActive(true);

        SkillData data = DataManager.Instance.skillDataTable[id];
        equippedSkill = SkillFactory.Create(id);

        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
        skillIcon.enabled = true;
        skillButton.onClick.AddListener(OnSkillButtonClicked);
    }

    private void OnSkillButtonClicked()
    {
        if (equippedSkill == null)
        {
            Debug.LogWarning("[ActiveSkillSlot] 장착된 스킬이 없습니다.");
            return;
        }


        //if (GameManager.Instance.player.CanUseSkill()) 
        //{
        //    equippedSkill.Execute(GameManager.Instance.player.gameObject);
        //    GameManager.Instance.player.TriggerGlobalCooldown(); 
        //}
    }
}
