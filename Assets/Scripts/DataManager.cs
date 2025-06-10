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
    public Dictionary<HUDType, HudNameData> HudNames = new Dictionary<HUDType, HudNameData>();
    public Dictionary<int, float> expTable = new Dictionary<int, float>();
    public Dictionary<EnemyId, EnemyData> enemyDataTable = new Dictionary<EnemyId, EnemyData>();

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
        LoadHUDName();
        LoadExpData();
        LoadEnemyData();
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

        Debug.Log($"[DataManager] statName : {statNames.Count}개의 데이터를 로드함");
    }

    private void LoadHUDName()
    {
        TextAsset HudNameData = Resources.Load<TextAsset>("CSV/HudNameData");
        string[] lines = HudNameData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            HUDType key = Enum.Parse<HUDType>(tokens[0].Trim());

            HudNameData data = new HudNameData
            {
                KR = tokens[1].Trim(),
                EN = tokens[2].Trim()
            };

            HudNames[key] = data;
        }

        Debug.Log($"[DataManager] hudNameData : {HudNames.Count}개의 데이터를 로드함");
    }

    private void LoadExpData()
    {
        TextAsset expData = Resources.Load<TextAsset>("CSV/ExpData");
        string[] lines = expData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            int level = int.Parse(tokens[0].Trim());
            float requiredExp = float.Parse(tokens[1].Trim());

            expTable[level] = requiredExp;
        }

        Debug.Log($"[DataManager] expTable : {expTable.Count}개의 데이터를 로드함");
    }

    public float GetExpData(int level)
    {
        if (expTable.ContainsKey(level) == false)
        {
            Debug.LogWarning($"[DataManager] 레벨 {level}에 대한 데이터가 없음");
            return 1000;
        }

        return expTable[level];
    }

    private void LoadEnemyData()
    {
        TextAsset enemyText = Resources.Load<TextAsset>("CSV/EnemyData");
        string[] lines = enemyText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            EnemyData data = new EnemyData
            {
                Id = Enum.Parse<EnemyId>(tokens[0].Trim()),
                Type = Enum.Parse<EnemyType>(tokens[1].Trim()),
                HP = float.Parse(tokens[2]),
                ATK = float.Parse(tokens[3]),
                DEF = float.Parse(tokens[4]),
                SPD = float.Parse(tokens[5]),
                Range = float.Parse(tokens[6]),
                AttackInterval = float.Parse(tokens[7]),
                EXP = float.Parse(tokens[8]),
            };

            enemyDataTable[data.Id] = data;
        }
        Debug.Log($"[DataManager] enemyDataTable : {enemyDataTable.Count}개의 데이터를 로드함");
    }
    public EnemyData GetEnemyData(EnemyId id)
    {
        if (enemyDataTable.TryGetValue(id, out var data) == true)
        {
            return data;
        }

        Debug.LogWarning($"[DataManager] EnemyId {id}에 해당하는 데이터가 없음");
        return null;
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

[System.Serializable]
public class HudNameData
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

[System.Serializable]
public class EnemyData
{
    public EnemyId Id;
    public EnemyType Type;
    public float HP;
    public float ATK;
    public float DEF;
    public float SPD;
    public float Range;
    public float AttackInterval;
    public float EXP;
}