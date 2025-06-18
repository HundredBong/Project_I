using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillPanel : MonoBehaviour
{
    [SerializeField] private ActiveSkillSlot[] skillSlots;

    public void Refresh(SkillId[] equippedSkills)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].Init(equippedSkills[i]);
        }
    }
}
