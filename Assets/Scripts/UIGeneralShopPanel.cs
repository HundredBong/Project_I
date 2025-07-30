using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGeneralShopPanel : MonoBehaviour
{
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private RectTransform _contentRoot;

    private void Start()
    {
        List<GeneralShopData> datas = DataManager.Instance.GetAllGeneralShopDatas();

        for (int i = 0; i < datas.Count; i++)
        {
            //contentPrefab을 root아래에 생성
            //GetComponent로 가져오고
            //datas[i]로 초기화해줌
        }
    }
}
