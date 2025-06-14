using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTestRunner : MonoBehaviour
{
    private SkillSlot skillSlot = new SkillSlot();

    private void Start()
    {
        skillSlot.Equip(SkillId.Lightning);
    }

    [ContextMenu("스킬 테스트")]
    public void SkillTest()
    {
        skillSlot.Use(gameObject);
    }
}
