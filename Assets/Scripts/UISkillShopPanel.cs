using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillShopPanel : MonoBehaviour
{
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private RectTransform _contentRoot;

    private List<UISkillShopSlot> _slots = new List<UISkillShopSlot>();

    private async void Start()
    {
        List<SkillShopData> datas = DataManager.Instance.GetAllSkillShppDatas();

        for (int i = 0; i < datas.Count; i++)
        {
            GameObject obj = Instantiate(_contentPrefab, _contentRoot);
            UISkillShopSlot slot = obj.GetComponent<UISkillShopSlot>();
            slot.Init(datas[i]);
            _slots.Add(slot);
        }

        long nowMs = await GameManager.Instance.statSaver.GetServerNowMsAsync();
        DateTime uiUtcDate = DateTimeOffset.FromUnixTimeMilliseconds(nowMs).UtcDateTime.Date;

        //서버 시간 기준으로 새로고침
        foreach (UISkillShopSlot slot in _slots)
        {
            slot.RefreshWithUtc(uiUtcDate);
        }
    }
}
