using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public Dictionary<StatType, StatNameData> statNames = new Dictionary<StatType, StatNameData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //-------------------------------------------

        LoadStatName();
    }

    private void LoadStatName()
    {
        TextAsset statNamaData = Resources.Load<TextAsset>("CSV/StatNameData");
        string[] lines = statNamaData.text.Split('\n');  

        //i를 1로 해야 헤더 안읽어옴, CSV라인 갯수만큼 쉼표단위로 읽어옴
        for (int i = 1; i < lines.Length; i++)
        {
            //비어있는 줄은 무시하기
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            //같은 줄에 여러 언어가 들어가있는 상태임, Split을 써서 쉼표 단위로 나눠주기
            string[] tokens = lines[i].Split(',');

            //CSV에 있는 Key값을 파싱해서 딕셔너리 키 생성
            //Trim으로 줄 양쪽 끝에 혹시 모를 \r같은 보이지않는 문자 제거
            StatType key = Enum.Parse<StatType>(tokens[0].Trim());

            //데이터 초기화
            StatNameData data = new StatNameData
            { 
                KR = tokens[1].Trim(),
                EN = tokens[2].Trim()
            };

            statNames[key] = data;
        }
    }
}

[System.Serializable]
public class StatNameData
{
    public string KR;
    public string EN;

    public string GetLocalizedText()
    {
        return LanguageManager.CurrentLanguage switch
        {
            LanguageType.KR => KR,
            LanguageType.EN => EN,
            _ => KR
        };
    }
}