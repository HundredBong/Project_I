using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
    public Dictionary<StatUpgradeType, StatNameData> statNames = new Dictionary<StatUpgradeType, StatNameData>();
    public Dictionary<HUDType, HudNameData> HudNames = new Dictionary<HUDType, HudNameData>();
    public Dictionary<int, float> expTable = new Dictionary<int, float>();
    public Dictionary<EnemyId, EnemyData> enemyDataTable = new Dictionary<EnemyId, EnemyData>();
    public Dictionary<int, StageData> stageDataTable = new Dictionary<int, StageData>();
    public Dictionary<SkillId, SkillData> skillDataTable = new Dictionary<SkillId, SkillData>();
    private Dictionary<string, LocalizedText> localizedTexts = new Dictionary<string, LocalizedText>();
    private Dictionary<GoldUpgradeType, GoldUpgradeData> goldUpgradeTable = new Dictionary<GoldUpgradeType, GoldUpgradeData>();
    private Dictionary<int, ItemData> itemDataTable = new Dictionary<int, ItemData>();

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

        LoadSpritesData();
        LoadLocalizedTexts();
        //LoadStatName();
        //LoadHUDName();
        LoadExpData();
        LoadEnemyData();
        LoadStageData();
        LoadSkillData();
        LoadGoldUpgradeData();
        LoadItemData();
    }

    private void LoadSpritesData()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprite");

        foreach (Sprite sprite in sprites)
        {
            Debug.Log($"{sprite.name}");
            spriteDic.Add(sprite.name, sprite);
        }

     
    }

    private void LoadLocalizedTexts()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("CSV/LocalizedTextData");
        string[] lines = textAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            if (tokens.Length < 3) continue;

            string key = tokens[0].Trim();

            LocalizedText text = new LocalizedText
            {
                KR = tokens[1].Trim(),
                EN = tokens[2].Trim()
            };

            localizedTexts[key] = text;
        }

        Debug.Log($"[DataManager] LocalizedText {localizedTexts.Count}�� �ε��");
    }

    public string GetLocalizedText(string key)
    {
        if (localizedTexts.TryGetValue(key, out LocalizedText text))
        {
            return text.Get();
        }

        Debug.LogWarning($"[DataManager] '{key}'�� �ش��ϴ� ���ö����� �ؽ�Ʈ�� ����");
        return key;
    }

    public string GetSkillDesc(SkillData data, SkillId id)
    {
        //����SkillInfoPopup���� �ű�

        //��� ��ų�� ������ ������ ������ �ƴϴٺ��� �Լ��� ���� ���� �ۼ���
        string rawText = DataManager.Instance.GetLocalizedText(data.DescKey);
        string formattedText = "";
        switch (id)
        {
            case SkillId.Lightning:
                //�ѱ���, ���� ��� ������ �����̶�� �����ϱ�� �ѵ�, ���� ����� �ٸ��ٸ� �� ���� �������� ��.
                //Ȥ�� �𸣴� ����ġ �ͽ��������� �ƴ� �Ϲ� ����ġ������ �ۼ�
                formattedText = string.Format(rawText, data.BaseValue, data.HitCount, data.StatucChance, data.Cooldown);
                return formattedText;
            default:
                return rawText;
        }
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
            StatUpgradeType key = Enum.Parse<StatUpgradeType>(tokens[0].Trim());

            //������ �ʱ�ȭ
            StatNameData data = new StatNameData
            {
                KR = tokens[1].Trim(),
                EN = tokens[2].Trim()
            };

            statNames[key] = data;
        }

        Debug.Log($"[DataManager] statName : {statNames.Count}���� �����͸� �ε���");
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

        Debug.Log($"[DataManager] hudNameData : {HudNames.Count}���� �����͸� �ε���");
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

        Debug.Log($"[DataManager] expTable : {expTable.Count}���� �����͸� �ε���");
    }

    public float GetExpData(int level)
    {
        if (expTable.ContainsKey(level) == false)
        {
            Debug.LogWarning($"[DataManager] ���� {level}�� ���� �����Ͱ� ����");
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
                Gold = float.Parse(tokens[9]),
            };

            enemyDataTable[data.Id] = data;
        }
        Debug.Log($"[DataManager] enemyDataTable : {enemyDataTable.Count}���� �����͸� �ε���");
    }

    public EnemyData GetEnemyData(EnemyId id)
    {
        if (enemyDataTable.TryGetValue(id, out var data) == true)
        {
            return data;
        }

        Debug.LogWarning($"[DataManager] EnemyId {id}�� �ش��ϴ� �����Ͱ� ����");
        return null;
    }

    private void LoadStageData()
    {
        TextAsset stageText = Resources.Load<TextAsset>("CSV/StageData");
        string[] lines = stageText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            int stageId = int.Parse(tokens[0]);

            string[] enemyIds = tokens[3].Trim().Split(';');
            List<EnemyId> enemyIdList = new List<EnemyId>();
            for (int h = 0; h < enemyIds.Length; h++)
            {
                enemyIdList.Add(Enum.Parse<EnemyId>(enemyIds[h].Trim()));
            }

            StageData data = new StageData()
            {
                StageId = stageId,
                StageType = Enum.Parse<StageType>(tokens[1].Trim()),
                BGM = tokens[2].Trim(),
                Enemies = enemyIdList,
                HPRate = float.Parse(tokens[4]),
                ATKRate = float.Parse(tokens[5]),
                DEFRate = float.Parse(tokens[6]),
                RewardRate = float.Parse(tokens[7]),
                InitCount = int.Parse(tokens[8]),
                AddCount = int.Parse(tokens[9])
            };

            stageDataTable[stageId] = data;
        }
        Debug.Log($"[DataManager] stageDataTable : {stageDataTable.Count}���� �����͸� �ε���");
    }

    private void LoadSkillData()
    {
        TextAsset skillText = Resources.Load<TextAsset>("CSV/SkillData");
        string[] lines = skillText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            SkillData data = new SkillData()
            {
                SkillId = Enum.Parse<SkillId>(tokens[0].Trim()),
                NameKey = tokens[1].Trim(),
                DescKey = tokens[2].Trim(),
                SkillIcon = tokens[3].Trim(),
                Grade = Enum.Parse<GradeType>(tokens[4].Trim()),
                Type = Enum.Parse<SkillType>(tokens[5].Trim()),
                Cooldown = float.Parse(tokens[6]),
                BaseValue = float.Parse(tokens[7]),
                BaseValueIncrease = float.Parse(tokens[8]),
                BufferDuration = float.Parse(tokens[9]),
                EffectType = Enum.Parse<SkillEffectType>(tokens[10].Trim()),
                PassiveValue = float.Parse(tokens[11]),
                PassiveEffectIncrease = float.Parse(tokens[12]),
                MaxLevel = int.Parse(tokens[13]),
                UpgradeCost = int.Parse(tokens[14]),
                UpgradeCostPerLevel = int.Parse(tokens[15]),
                AwakenRequiredCount = tokens[16].Trim().Split(';').Select(int.Parse).ToArray(),
                StatusEffect = Enum.Parse<StatusEffectType>(tokens[17].Trim()),
                StatucChance = float.Parse(tokens[18]),
                HitCount = int.Parse(tokens[19]),
                TargetCount = int.Parse(tokens[20]),
                isUnlocked = false
            };
            skillDataTable[data.SkillId] = data;
        }
        Debug.Log($"[DataManager] skillDataTable : {skillDataTable.Count}���� �����͸� �ε���");
    }

    public List<SkillData> GetAllSkillData()
    {
        List<SkillData> skillList = new List<SkillData>();

        foreach (KeyValuePair<SkillId, SkillData> kvp in skillDataTable)
        {
            skillList.Add(kvp.Value);
        }

        return skillList;
    }

    private void LoadGoldUpgradeData()
    {
        TextAsset goldUpgradeText = Resources.Load<TextAsset>("CSV/GoldUpgradeData");
        string[] lines = goldUpgradeText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            GoldUpgradeType type = Enum.Parse<GoldUpgradeType>(tokens[0].Trim());

            GoldUpgradeData goldUpgradeData = new GoldUpgradeData()
            {
                GoldUpgradeType = type,
                NameKey = tokens[1].Trim(),
                MaxLevel = int.Parse(tokens[2].Trim()),
                BaseValue = float.Parse(tokens[3].Trim()),
                BaseValueIncrease = float.Parse(tokens[4].Trim()),
                StatIcon = tokens[5].Trim(),
                Price = float.Parse(tokens[6].Trim()),
                PriceIncrease = float.Parse(tokens[7].Trim()),
            };

            goldUpgradeTable[type] = goldUpgradeData;
        }
        Debug.Log($"[DataManager] GoldUpgradeData : {skillDataTable.Count}���� �����͸� �ε���");
    }

    public GoldUpgradeData GetGoldUpgradeData(GoldUpgradeType type)
    {
        if (goldUpgradeTable.TryGetValue(type, out GoldUpgradeData data))
        {
            return data;
        }

        Debug.LogWarning($"[DataManager] GoldUpgradeType {type}�� �ش��ϴ� �����Ͱ� ����");
        return null;
    }

    private void LoadItemData()
    {
        TextAsset itemText = Resources.Load<TextAsset>("CSV/ItemData");
        string[] lines = itemText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');
            int id = int.Parse(tokens[0].Trim());

            ItemData itemDate = new ItemData()
            {
                Id = id,
                ItemType = Enum.Parse<ItemType>(tokens[1].Trim()),
                GradeType = Enum.Parse<GradeType>(tokens[2].Trim()),
                Stage = int.Parse(tokens[3].Trim()),
                MaxLevel = int.Parse(tokens[4].Trim()),
                BaseValue = float.Parse(tokens[5].Trim()),
                BaseValuePerLevel = float.Parse(tokens[6].Trim()),
                OwnedValue = float.Parse(tokens[7].Trim()),
                OwnedValuePerLevel = float.Parse(tokens[8].Trim()),
                UpgradePrice = float.Parse(tokens[9].Trim()),
                NameKey = tokens[10].Trim(),
                IconKey = tokens[11].Trim(),
            };

            itemDataTable[id] = itemDate;
        }

        Debug.Log($"[DataManager] {itemDataTable.Count}�� ������ �����Ͱ� �ε��");
    }

    public Dictionary<int, ItemData> GetItemData()
    {
        return itemDataTable;
    }

    public Sprite GetSpriteByKey(string key)
    {
        if (spriteDic.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning("[DataManager] �ش��ϴ� Ű�� ��������Ʈ�� ã�� �� ����");
            return null;
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
    public float Gold;
}

[System.Serializable]
public class StageData
{
    public int StageId;
    public StageType StageType;
    public string BGM;
    public List<EnemyId> Enemies;
    public float HPRate;
    public float ATKRate;
    public float DEFRate;
    public float RewardRate;
    public int InitCount;
    public int AddCount;
}

[System.Serializable]
public class SkillData
{
    public SkillId SkillId;
    public string NameKey; //string, string ��ųʸ� ���� �ҷ����� �뵵
    public string DescKey; //string, string ��ųʸ� ���� �ҷ����� �뵵
    public string SkillIcon; //���� DataManager���� SpriteDictionary�� ������ ����
    public GradeType Grade; //���, Common, Uncommon, Rare, Epic, Legendary, Mythical
    public SkillType Type; //Active, Buff, Passive
    public float Cooldown;//��ų ��Ÿ��, �� ����
    public float BaseValue;//��ų�� �⺻ ��, ���� ��� ���ݷ� ����, ü�� ȸ�� ��
    public float BaseValueIncrease; //������ �� �����ϴ� �⺻ ��
    public float BufferDuration; //���� ���� �ð�, �� ����, ���� ��ų���� ����
    public SkillEffectType EffectType; //��ų ȿ�� Ÿ��, GoldBonus, ExpBonus ��
    public float PassiveValue; //���� ȿ��
    public float PassiveEffectIncrease; //���� ȿ�� ������
    public int MaxLevel; //��ų�� �ִ� ����
    public int UpgradeCost; //��ų ���׷��̵� ���, ��� ��
    public int UpgradeCostPerLevel; //������ ���׷��̵� ��� ������
    public int[] AwakenRequiredCount;
    public StatusEffectType StatusEffect;
    public float StatucChance;
    public int HitCount;
    public int TargetCount;
    public bool isUnlocked;
}

public class LocalizedText
{
    public string KR;
    public string EN;

    public string Get()
    {
        return LanguageManager.CurrentLanguage switch
        {
            LanguageType.KR => KR,
            LanguageType.EN => EN,
            _ => KR
        };
    }
}

public class GoldUpgradeData
{
    public GoldUpgradeType GoldUpgradeType;
    public string NameKey;
    public int MaxLevel;
    public float BaseValue;
    public float BaseValueIncrease;
    public string StatIcon;
    public float Price;
    public float PriceIncrease;
}

public class ItemData
{
    public int Id;
    public ItemType ItemType;
    public GradeType GradeType;
    public int Stage;
    public int MaxLevel;
    public float BaseValue;
    public float BaseValuePerLevel;
    public float OwnedValue;
    public float OwnedValuePerLevel;
    public float UpgradePrice;
    public string NameKey;
    public string IconKey;
}

[System.Serializable]
public class InventoryItem
{
    public ItemData Data;
    public int Level;
    public int Count;
    public bool IsEquipped;
    public bool IsUnlocked;

    public InventoryItem(ItemData data, bool isUnlocked = false)
    {
        Data = data;
        Level = 1;
        Count = 1;
        IsEquipped = false;
        IsUnlocked = isUnlocked;
    }
}