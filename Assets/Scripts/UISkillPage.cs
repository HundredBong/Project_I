using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillPage : UIPage
{
    //UI페이지에 닫기버튼이랑 열기닫기 기능은 있음
    //열기 버튼은 다른 UI에 붙어있음
    //그럼 여기서 해야할 일은 UIStatPage처럼 DataManager의 skillDataTable을 순회하며 오브젝트 생성
    //스킬 레벨에 변동이 있을 때마다 업데이트

    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private Transform contentRoot;

    private List<SkillSlotUI> slotUIs = new List<SkillSlotUI>();

    private void Start()
    {
        foreach (KeyValuePair<SkillId, SkillData> kvp in DataManager.Instance.skillDataTable)
        {
            GameObject obj = Instantiate(skillSlotPrefab, contentRoot);
            SkillSlotUI ui = obj.GetComponent<SkillSlotUI>();
            ui.Init(kvp.Value);
            slotUIs.Add(ui);
        }

        Debug.Log("[UISkillPage] 스킬 슬롯 초기화됨");
    }

    public void Refresh()
    {
        foreach (SkillSlotUI ui in slotUIs)
        {
            ui.Refresh();
        }
    }
}
