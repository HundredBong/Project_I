using System;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<SkillId, SkillData> skillDataTable = new Dictionary<SkillId, SkillData>();
    private Dictionary<string, LocalizedText> localizedTexts = new Dictionary<string, LocalizedText>();
    private Dictionary<GoldUpgradeType, GoldUpgradeData> goldUpgradeTable = new Dictionary<GoldUpgradeType, GoldUpgradeData>();
    private Dictionary<int, ItemData> itemDataTable = new Dictionary<int, ItemData>();
    private Dictionary<SummonSubCategory, List<int>> summonExpDatas = new Dictionary<SummonSubCategory, List<int>>();
    private Dictionary<SummonSubCategory, Dictionary<int, SummonRateData>> summonRateTable = new Dictionary<SummonSubCategory, Dictionary<int, SummonRateData>>();
    private Dictionary<SummonSubCategory, Dictionary<int, SummonRewardData>> summonRewardTable = new Dictionary<SummonSubCategory, Dictionary<int, SummonRewardData>>();

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
        LoadSummonExpData();
        LoadSummonGradeProbabilities();
        LoadSummonStageProbabilities();
        LoadSummonRewardData();
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
                PassiveValuePerLevel = float.Parse(tokens[12]),
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

    public SkillData GetSkill(SkillId skillId)
    {
        return skillDataTable[skillId];
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
                UpgradePrice = int.Parse(tokens[9].Trim()),
                NameKey = tokens[10].Trim(),
                IconKey = tokens[11].Trim(),
                EquippedEffectType = Enum.Parse<SkillEffectType>(tokens[12].Trim()),
                OwnedEffectType = Enum.Parse<SkillEffectType>(tokens[13].Trim()),
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

    private void LoadSummonExpData()
    {
        TextAsset expText = Resources.Load<TextAsset>("CSV/SummonExpData");
        string[] lines = expText.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');
            SummonSubCategory category = Enum.Parse<SummonSubCategory>(tokens[0].Trim());

            string[] exps = tokens[1].Trim().Split(';');
            List<int> expList = new List<int>();

            for (int h = 0; h < exps.Length; h++)
            {
                expList.Add(int.Parse(exps[h].Trim()));
            }

            summonExpDatas[category] = expList;
        }

        Debug.Log($"[DataManager] ��ųʸ� {summonExpDatas.Count}, ����Ʈ{summonExpDatas.Values.Count}�� �����͸� �ε���");
    }

    public int GetSummonMaxExp(SummonSubCategory category, int currentLevel)
    {
        if (summonExpDatas.TryGetValue(category, out var expData))
        {
            //�ּ� 0, �ִ� ����Ʈ ũ�� - 1
            int index = Mathf.Clamp(currentLevel - 1, 0, expData.Count - 1);
            return expData[index];
        }

        return 1000;
    }

    private void LoadSummonGradeProbabilities()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("CSV/GradeProbabilities");
        string[] lines = textAsset.text.Split('\n');
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] token = lines[i].Split(',');

            //ī�װ�, ���� �Ľ�
            SummonSubCategory category = Enum.Parse<SummonSubCategory>(token[0]);
            int level = int.Parse(token[1]);

            SummonRateData rateData = new SummonRateData()
            {
                Level = level,
            };

            //��� �Ľ�
            for (int h = 2; h < token.Length; h++)
            {
                //���[2]�� Common
                string grade = headers[h];

                if (Enum.TryParse(grade, out GradeType gradeType))
                {
                    //��ū[2]�� 0.5
                    if (float.TryParse(token[h], out float probability))
                    {
                        //Ȯ�� ��ųʸ� �ʱ�ȭ
                        rateData.GradeProbabilities[gradeType] = probability;
                    }
                    else
                    {
                        Debug.LogWarning($"[DataManager] Ȯ�� �Ľ� ������, {token[h]}");
                    }
                }
            }

            //��ųʸ��� ī�װ��� ������ �����Ͱ� �������� �ʴ´ٸ�
            if (summonRateTable.TryGetValue(category, out var table) == false)
            {
                table = new Dictionary<int, SummonRateData>();
                //ī�װ��� ���� ���� ��ųʸ� �߰�
                summonRateTable.Add(category, table);
            }

            //���� ��ųʸ��� ������ ������ �߰�
            table[level] = rateData;
        }
        Debug.Log($"[DataManager] ��ȯ ��� Ȯ�� �ε� ��, {summonRateTable.Count}");
    }

    private void LoadSummonStageProbabilities()
    {
        TextAsset asset = Resources.Load<TextAsset>("CSV/StageProbabilities");
        string[] lines = asset.text.Split('\n');
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            SummonSubCategory category = Enum.Parse<SummonSubCategory>(tokens[0]);
            int level = int.Parse(tokens[1]);

            //ī�װ��� �ش��ϴ� �����Ͱ� ���ų�, ī�װ��� ������ �ش��ϴ� �����Ͱ� ���ٸ�
            if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData rateData) == false)
            {
                Debug.LogWarning($"[DataManager] {category} {level} ������ �ش��ϴ� ��ȯ ���� �����Ͱ� ����");
                continue;
            }

            for (int h = 2; h < tokens.Length; h++)
            {
                string grade = headers[h];

                if (Enum.TryParse(grade, out GradeType gradeType) == false) { continue; }

                string[] stageTokens = tokens[h].Split(';');

                Dictionary<int, float> stageProb = new Dictionary<int, float>();

                for (int j = 0; j < stageTokens.Length; j++) //s
                {
                    if (float.TryParse(stageTokens[j], out float p))
                    {
                        //1�ܰ���� �����Ϸ��� +1 ��
                        stageProb[j + 1] = p;
                        //Weapon Level 1, Common, { 0.6, 0.4 }
                    }
                    else
                    {
                        Debug.Log($"[DataManager] �߸��� �ܰ� �Ľ�, {stageTokens[j]}");
                    }
                }

                rateData.StageProbabilities[gradeType] = stageProb;
            }
        }
        Debug.Log("[DataManager] StageProbabilities �Ľ̵�");
    }

    public GradeType GetRandomGrade(SummonSubCategory category, int level)
    {
        if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData data) == false)
        {
            Debug.LogWarning($"[DataManager] {category}, {level}�� �ش��ϴ� ��� ����");
            return GradeType.Common;
        }

        Dictionary<GradeType, float> gradeProb = data.GradeProbabilities;
        float total = 0f;

        //csv���� total�� 1�� �ƴҼ��� �����Ƿ� ���� �����
        foreach (float p in gradeProb.Values)
        {
            total += p;
        }

        float rand = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        //foreach (KeyValuePair<GradeType, float> kvp in gradeProb)
        //{
        //    if (rand <= cumulative)
        //    {
        //        return kvp.Key;
        //    }
        //}

        //��ųʸ��� ���Լ����� �������� �����Ƿ� ���� ����� ��� ��.
        //���ο� ����� �߰��ɽ� ���⵵ �߰������ ��
        GradeType[] gradeOrder = { GradeType.Common, GradeType.Uncommon, GradeType.Rare, GradeType.Epic, GradeType.Legendary, GradeType.Mythical };

        foreach (GradeType grade in gradeOrder)
        {
            if (gradeProb.TryGetValue(grade, out float p))
            {
                cumulative += p;
                if (rand <= cumulative)
                {
                    return grade;
                }
            }
        }

        Debug.LogWarning($"[DataManager] {category} {level} Ȯ�� ��� ������, �⺻�� ��ȯ");
        return GradeType.Common;
    }

    public int GetRandomStage(SummonSubCategory category, int level, GradeType grade)
    {
        if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData data) == false)
        {
            Debug.LogWarning($"[DataManager] {category}, {level}�� �ش��ϴ� ���� ����");
            return 1;
        }

        if (data.StageProbabilities.TryGetValue(grade, out var stageData) == false)
        {
            Debug.LogWarning($"[DataManager] {grade} ����� Ȯ�� ���� ����");
            return 1;
        }

        float total = 0f;
        foreach (float p in stageData.Values)
        {
            total += p;
        }

        float rand = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        //foreach (var kvp in stageData)
        //{
        //    cumulative += kvp.Value;
        //    if (rand <= cumulative)
        //    {
        //        return kvp.Key;
        //    }
        //}

        //��ųʸ� ���� �̽��� Ű ������ ��� ��
        foreach (int stage in stageData.Keys.OrderBy(k => k))
        {
            cumulative += stageData[stage];
            if (rand <= cumulative)
            {
                return stage;
            }
        }

        Debug.LogWarning($"[DataManager] �ܰ� ���� ����, �⺻�� ��ȯ");
        return 1;
    }

    public int GetRandomItemId(SummonSubCategory category, GradeType grade, int stage)
    {
        ItemType type = ConvertToItemType(category);

        List<ItemData> items = new List<ItemData>();

        foreach (var kvp in itemDataTable)
        {
            ItemData data = kvp.Value;

            if (data.ItemType == type && data.GradeType == grade && data.Stage == stage)
            {
                items.Add(data);
            }
        }

        if (items.Count == 0)
        {
            Debug.LogWarning("[DataManager] ���ǿ� �´� ������ ����");
            return 0;
        }


        int index = UnityEngine.Random.Range(0, items.Count);
        return items[index].Id;
    }

    private ItemType ConvertToItemType(SummonSubCategory category)
    {
        return category switch
        {
            SummonSubCategory.Weapon => ItemType.Weapon,
            SummonSubCategory.Armor => ItemType.Armor,
            SummonSubCategory.Necklace => ItemType.Necklace,
            _ => throw new ArgumentException()
        };
    }

    public SkillId GetRandomSkillId(SummonSubCategory category, GradeType grade)
    {
        if (category != SummonSubCategory.Skill)
        {
            Debug.LogWarning($"[DataManager] ī�װ��� Skill�� �ƴ� : {category}");
            return SkillId.None;
        }

        List<SkillData> skills = new List<SkillData>();

        foreach (var kvp in skillDataTable)
        {
            SkillData skill = kvp.Value;

            if (skill.Grade == grade && skill.SkillId != SkillId.None)
            {
                skills.Add(skill);
            }
        }

        if (skills.Count == 0)
        {
            Debug.Log($"[DataManage] {grade}����� ��ų�� ����");
            return SkillId.None;
        }

        int index = UnityEngine.Random.Range(0, skills.Count);
        return skills[index].SkillId;
    }

    private void LoadSummonRewardData()
    {
        TextAsset asset = Resources.Load<TextAsset>("CSV/SummonLevelRewardData");
        string[] lines = asset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }
            Debug.Log($"[����{i}] {lines[i]}");
            string[] tokens = lines[i].Split(',');

            SummonSubCategory category = Enum.Parse<SummonSubCategory>(tokens[0].Trim());
            int level = int.Parse(tokens[1].Trim());

            SummonRewardData data = new SummonRewardData()
            {
                SubCategory = category,
                Level = level,
                RewardType = Enum.Parse<RewardType>(tokens[2].Trim()),
                Id = tokens[3].Trim(),
                Amount = int.Parse(tokens[4].Trim())
            };

            if (summonRewardTable.TryGetValue(category, out var table) == false)
            {
                table = new Dictionary<int, SummonRewardData>();
                summonRewardTable[category] = table;
            }

            table[level] = data;
        }

        Debug.Log($"[DataManager] ��ȯ ���� ���� ������ �ε��, {summonRewardTable.Count}");
    }

    public SummonRewardData GetRewardData(SummonSubCategory category, int level)
    {
        if (summonRewardTable.TryGetValue(category, out var table) && table.TryGetValue(level, out SummonRewardData data))
        {
            return data;
        }
        Debug.LogWarning($"[DataManager] {category} {level} ������ ���� ���� �����Ͱ� ����");
        return null;
    }

    //public float GetExpData(int level)
    //{
    //    if (expTable.ContainsKey(level) == false)
    //    {
    //        Debug.LogWarning($"[DataManager] ���� {level}�� ���� �����Ͱ� ����");
    //        return 1000;
    //    }

    //    return expTable[level];
    //}
    //private void Test()
    //{
    //    float a = summonRateTable[SummonSubCategory.Weapon][1].GradeProbabilities[GradeType.Uncommon];
    //    float b = summonRateTable[SummonSubCategory.Weapon][1].StageProbabilities[GradeType.Uncommon][1];
    //}
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
    public float PassiveValuePerLevel; //���� ȿ�� ������
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
    public int UpgradePrice;
    public string NameKey;
    public string IconKey;
    public SkillEffectType EquippedEffectType;
    public SkillEffectType OwnedEffectType;
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

public class SummonRateData
{
    public int Level;

    //���� �� ����� �����ִ� Ȯ��
    public Dictionary<GradeType, float> GradeProbabilities = new Dictionary<GradeType, float>();

    //��� �ȿ��� �ܰ躰 Ȯ��
    //Common, 1�ܰ�� 40%, 2�ܰ�� 30%
    public Dictionary<GradeType, Dictionary<int, float>> StageProbabilities = new Dictionary<GradeType, Dictionary<int, float>>();
}

public class SummonRewardData
{
    public SummonSubCategory SubCategory;
    public int Level;
    public RewardType RewardType;
    public string Id;
    public int Amount;
}