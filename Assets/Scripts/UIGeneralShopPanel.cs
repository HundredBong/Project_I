using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGeneralShopPanel : MonoBehaviour
{
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private RectTransform _contentRoot;

    private List<UIGeneralShopSlot> _slots = new List<UIGeneralShopSlot>();

    private void OnEnable()
    {
        //LanguageManager.OnLanguageChanged += 
    }

    private void OnDisable()
    {
        //LanguageManager.OnLanguageChanged -=
    }

    private void Start()
    {
        List<GeneralShopData> datas = DataManager.Instance.GetAllGeneralShopDatas();

        for (int i = 0; i < datas.Count; i++)
        {
            GameObject obj = Instantiate(_contentPrefab, _contentRoot);
            UIGeneralShopSlot slot = obj.GetComponent<UIGeneralShopSlot>();
            slot.Init(datas[i]);
            _slots.Add(slot);
        }
    }
}
