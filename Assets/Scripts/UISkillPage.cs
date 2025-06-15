using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillPage : UIPage
{
    //UI�������� �ݱ��ư�̶� ����ݱ� ����� ����
    //���� ��ư�� �ٸ� UI�� �پ�����
    //�׷� ���⼭ �ؾ��� ���� UIStatPageó�� DataManager�� skillDataTable�� ��ȸ�ϸ� ������Ʈ ����
    //��ų ������ ������ ���� ������ ������Ʈ

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

        Debug.Log("[UISkillPage] ��ų ���� �ʱ�ȭ��");
    }

    public void Refresh()
    {
        foreach (SkillSlotUI ui in slotUIs)
        {
            ui.Refresh();
        }
    }
}
