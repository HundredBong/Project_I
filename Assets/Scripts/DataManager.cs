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

        //i�� 1�� �ؾ� ��� ���о��, CSV���� ������ŭ ��ǥ������ �о��
        for (int i = 1; i < lines.Length; i++)
        {
            //����ִ� ���� �����ϱ�
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            //���� �ٿ� ���� �� ���ִ� ������, Split�� �Ἥ ��ǥ ������ �����ֱ�
            string[] tokens = lines[i].Split(',');

            //CSV�� �ִ� Key���� �Ľ��ؼ� ��ųʸ� Ű ����
            //Trim���� �� ���� ���� Ȥ�� �� \r���� �������ʴ� ���� ����
            StatType key = Enum.Parse<StatType>(tokens[0].Trim());

            //������ �ʱ�ȭ
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