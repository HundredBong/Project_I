using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
    private Dictionary<string, AudioClip> audioDic = new Dictionary<string, AudioClip>();
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
    private List<GeneralShopData> generalShopDataList = new List<GeneralShopData>();
    private List<SkillShopData> skillShopDataList = new List<SkillShopData>();
    private Dictionary<DungeonType, DungeonData> dungeonDataTable = new Dictionary<DungeonType, DungeonData>();
    private Dictionary<DungeonType, Dictionary<int, DungeonLevelData>> dungeonLevelDataTable = new Dictionary<DungeonType, Dictionary<int, DungeonLevelData>>();
    private Dictionary<SummonSubCategory, Dictionary<int, int>> summonPriceDataTable = new Dictionary<SummonSubCategory, Dictionary<int, int>>();

    private const string MANIFEST_URL = "https://hundredbong.github.io/Project_I_hotdata/hotdata/manifest.json";

    public bool IsReady { get; private set; } = false;

    private UniTaskCompletionSource _readyTcs;

    private string _localDir;
    private string _localManifestPath;

    [SerializeField] private bool loadByResources = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //로컬 캐시 루트 경로 지정, persistentDataPath : 플랫폼별 안전한 쓰기 위치
        _localDir = Path.Combine(Application.persistentDataPath, "hotdata");

        //OS에 맞게 / 또는 \ 자동으로 입력
        _localManifestPath = Path.Combine(_localDir, "manifest.json");

        try
        {
            //로컬 경로에 폴더가 존재하지 않는다면
            if (Directory.Exists(_localDir) == false)
            {
                //폴더 새로 생성
                Directory.CreateDirectory(_localDir);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] 로컬 디렉토리 생성 실패, {e.Message}");
        }

        _readyTcs = new UniTaskCompletionSource();

        //-------------------------------------------

        LoadSpritesData();
        LoadAudioData();

        if (loadByResources)
        {
            LoadLocalizedTexts();
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
            LoadGeneralShopData();
            LoadSkillShopData();
            LoadDungeonData();
            LoadDungeonLevelData();
            LoadSummonPriceData();
        }

    }

    public async UniTask InitAsync()
    {
        if (loadByResources)
        {
            _readyTcs.TrySetResult();
            return;
        }

        HotdataManifest remote = await TryDownloadManifestAsync();
        Debug.Log($"[DataManager] 원격 manifest 다운로드 {(remote != null ? "성공" : "실패")}");

        HotdataManifest local = await TryLoadLocalManifestAsync();
        Debug.Log($"[DataManager] 로컬 manifest {(local != null ? "존재함" : "없음")}");

        if (remote != null)
        {
            Debug.Log("[DataManager] CompareAndUpdateAsync 호출");
            await CompareAndUpdateAsync(remote, local);
            Debug.Log("[DataManager] CompareAndUpdateAsync 완료");

            //서버에서 받은 매니페스트를 json 문자열로 직렬화
            await File.WriteAllTextAsync(_localManifestPath, JsonUtility.ToJson(remote, true), System.Text.Encoding.UTF8);
            Debug.Log("[DataManager] 로컬 manifest 저장 완료");
        }
        else
        {
            //원격 매니페스트 실패 -> 로컬 매니페스트 사용,
            //로컬 매니페스트도 없으면 리소스 사용
        }

        LoadLocalizedTexts();
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
        LoadGeneralShopData();
        LoadSkillShopData();
        LoadDungeonData();
        LoadDungeonLevelData();
        LoadSummonPriceData();

        Debug.Log("[DataManager] LoadData 완료");

        IsReady = true;
        _readyTcs.TrySetResult();
    }

    private async UniTask<HotdataManifest> TryDownloadManifestAsync()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string json = await client.GetStringAsync(MANIFEST_URL);
                HotdataManifest manifest = JsonUtility.FromJson<HotdataManifest>(json);
                return manifest;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataManager] 매니페스트 다운로드 실패 : {e.Message}");
            return null;
        }
    }

    private async UniTask<HotdataManifest> TryLoadLocalManifestAsync()
    {
        try
        {
            //매니페스트 파일이 존재하는지 확인
            if (File.Exists(_localManifestPath))
            {
                string json = await File.ReadAllTextAsync(_localManifestPath, Encoding.UTF8);
                HotdataManifest hotdata = JsonUtility.FromJson<HotdataManifest>(json);
                return hotdata;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataManager] 로컬 매니페스트 불러오기 실패 : {e.Message}");
        }
        return null;
    }

    private async UniTask CompareAndUpdateAsync(HotdataManifest remote, HotdataManifest local)
    {
        if (remote.file == null || remote.file.Length == 0)
        {
            return;
        }

        foreach (HotdataFile remoteFile in remote.file)
        {
            bool needDownload = true;
            string localPath = Path.Combine(_localDir, remoteFile.name);
            if (local != null && local.file != null)
            {
                foreach (HotdataFile localFile in local.file)
                {
                    bool sameName = string.Equals(remoteFile.name, localFile.name);

                    //이름부터가 다르면 바로 다음 루프로
                    if (sameName == false)
                    {
                        continue;
                    }

                    bool sameVersion = string.Equals(remoteFile.version, localFile.version, StringComparison.OrdinalIgnoreCase);
                    bool sameSchema = string.Equals(remoteFile.schemaVersion, localFile.schemaVersion, StringComparison.OrdinalIgnoreCase);
                    bool sameHash = string.Equals(remoteFile.sha256, localFile.sha256);
                    bool sameSize = remoteFile.size == localFile.size;

                    bool fileExists = File.Exists(localPath);

                    if (sameVersion && sameSchema && sameHash && sameSize && fileExists)
                    {
                        Debug.Log("[DataManager] 동일한 파일, 다운로드 생략");
                        needDownload = false;
                        break;
                    }
                }
            }

            if (!needDownload) continue;

            Debug.Log($"[DataManager] {remoteFile.name} 다운로드 필요");

            bool ok = await DownloadAndVerifyAsync(remoteFile.url, remoteFile.sha256, remoteFile.size, localPath);

            if (ok == false)
            {
                Debug.LogWarning($"[DataManager] 다운로드에 실패함, {remoteFile.name}");
            }
        }
    }

    private async UniTask<bool> DownloadAndVerifyAsync(string url, string expectedSha256, long expectedSize, string savePath)
    {
        //persistanceDataPath/hotdata/SomeData.csv.tmp 
        string tmpPath = savePath + ".tmp";

        Debug.Log($"[DataManager] {Path.GetFileName(savePath)} 다운로드 시작: {url}");

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);

                byte[] bytes = await client.GetByteArrayAsync(url);

                Debug.Log($"[DataManager] {Path.GetFileName(savePath)} 다운로드 완료 ({bytes.Length} bytes)");

                //받은 파일의 크기가 동일하지 않다면
                if (expectedSize > 0 && bytes.LongLength != expectedSize)
                {
                    Debug.LogWarning($"[DataManager] 다운로드받은 파일의 크기가 일치하지 않음. 기대 크기 : {expectedSize}, 받은 파일 크기 : {bytes.LongLength}, url : {url}");
                    return false;
                }

                if (string.IsNullOrEmpty(expectedSha256) == false)
                {
                    string got = ComputeSha256Hex(bytes);

                    if (StringComparer.OrdinalIgnoreCase.Equals(got, expectedSha256) == false)
                    {
                        Debug.LogWarning($"[DataManager] SHA256 키가 맞지 않음. 기대 값 : {expectedSha256}, 받은 값 : {got}, url : {url}");
                        return false;
                    }
                }

                await File.WriteAllBytesAsync(tmpPath, bytes);

                //로컬에 이미 파일이 존재한다면 
                if (File.Exists(savePath))
                {
                    //기존 파일 삭제
                    File.Delete(savePath);
                }
                //.csv.tmp 에서 .csv로 변경
                File.Move(tmpPath, savePath);

                Debug.Log($"[DataManager] {Path.GetFileName(savePath)} 저장 완료");

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataManager] Download err: {e.Message}, url : {url}");

            try
            {
                //임시 파일 제거
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
            }
            catch { }

            return false;
        }
    }

    private string ComputeSha256Hex(byte[] data)
    {
        using (SHA256 sha = SHA256.Create())
        {
            //ComputeHash 돌리면 크기가 32인 문자열 배열 생성
            //16진수로 바꾸는 과정에서 바이트 하나가 문자 2개가 됨
            byte[] hash = sha.ComputeHash(data);

            //표기 과정에서 리사이즈가 일어나니 미리 정해둠, 사실상 64
            StringBuilder sb = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                //1바이트를 16진수로 표기
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }

    private string[] LoadCsvLines(string fileName)
    {
        if (loadByResources)
        {
            string resourceKey = Path.GetFileNameWithoutExtension(fileName);

            TextAsset asset = Resources.Load<TextAsset>($"CSV/{resourceKey}");

            if (asset == null)
            {
                Debug.LogWarning($"[DataManager] CSV 리소스 없음: {fileName}");
                return Array.Empty<string>();
            }

            return asset.text.Split('\n');
        }
        else
        {
            string cachedPath = Path.Combine(_localDir, fileName);

            //persistentDataPath/hotdata에 있는 파일 확인
            if (File.Exists(cachedPath))
            {
                //있으면 한 줄씩 읽어서 반환
                return File.ReadAllLines(cachedPath, Encoding.UTF8);
            }

            //없다면 Resources/CSV에서 읽어오기

            //csv확장자 버리고 가져옴
            string resourceKey = Path.GetFileNameWithoutExtension(fileName);

            TextAsset asset = Resources.Load<TextAsset>($"CSV/{resourceKey}");

            if (asset == null)
            {
                Debug.LogWarning($"[DataManager] CSV 리소스 없음: {fileName}");
                return Array.Empty<string>();
            }

            return asset.text.Split('\n');
        }

    }

    private void LoadSpritesData()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprite");

        foreach (Sprite sprite in sprites)
        {
            //Debug.Log($"{sprite.name}");
            spriteDic.Add(sprite.name, sprite);
        }
    }

    private void LoadAudioData()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");

        foreach (AudioClip clip in clips)
        {
            audioDic.Add(clip.name, clip);
            Debug.Log(clip.name);
        }
    }


    private void LoadLocalizedTexts()
    {
        //TextAsset textAsset = Resources.Load<TextAsset>("CSV/LocalizedTextData");
        //string[] lines = textAsset.text.Split('\n');
        string[] lines = LoadCsvLines("LocalizedTextData.csv");

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

        //Debug.Log($"[DataManager] LocalizedText {localizedTexts.Count}개 로드됨");
    }

    public string GetLocalizedText(string key, Dictionary<string, string> args = null)
    {
        if (localizedTexts.TryGetValue(key, out LocalizedText text))
        {
            string result = text.Get();

            if (args != null)
            {
                foreach (var kvp in args)
                {
                    result = result.Replace("{" + kvp.Key + "}", kvp.Value.ToString());
                }
            }
            return result.Replace('|', ',');
        }

        Debug.LogWarning($"[DataManager] '{key}'에 해당하는 로컬라이즈 텍스트가 없음");
        return key;
    }

    public string GetSkillDesc(SkillData data, SkillId id)
    {
        //string, object타입보다는 string, string으로 하는게 박싱 적음
        Dictionary<string, string> args = new Dictionary<string, string>
        {
            { "damage", SkillManager.Instance.CalculateSkillDamage(data).ToString("F0") },
            { "hitCount", ((int)data.HitCount).ToString() },
            { "targetCount", ((int)data.TargetCount).ToString() },
            { "chance", ((float)data.StatucChance).ToString("F0") },
            { "cooldown", ((float)data.Cooldown).ToString("F0") }
        };

        string desc = GetLocalizedText(data.DescKey, args);
        return desc.Replace('|', ',');
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
            StatUpgradeType key = Enum.Parse<StatUpgradeType>(tokens[0].Trim());

            //데이터 초기화
            StatNameData data = new StatNameData
            {
                KR = tokens[1].Trim(),
                EN = tokens[2].Trim()
            };

            statNames[key] = data;
        }

        //Debug.Log($"[DataManager] statName : {statNames.Count}개의 데이터를 로드함");
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

        //Debug.Log($"[DataManager] hudNameData : {HudNames.Count}개의 데이터를 로드함");
    }

    private void LoadExpData()
    {
        //TextAsset expData = Resources.Load<TextAsset>("CSV/ExpData");
        //string[] lines = expData.text.Split('\n');
        string[] lines = LoadCsvLines("ExpData.csv");
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            int level = int.Parse(tokens[0].Trim());
            float requiredExp = float.Parse(tokens[1].Trim());

            expTable[level] = requiredExp;
        }

        //Debug.Log($"[DataManager] expTable : {expTable.Count}개의 데이터를 로드함");
    }

    public float GetExpData(int level)
    {
        if (expTable.ContainsKey(level) == false)
        {
            //Debug.LogWarning($"[DataManager] 레벨 {level}에 대한 데이터가 없음");
            return 1000;
        }

        return expTable[level];
    }

    private void LoadEnemyData()
    {
        //TextAsset enemyText = Resources.Load<TextAsset>("CSV/EnemyData");
        //string[] lines = enemyText.text.Split('\n');
        string[] lines = LoadCsvLines("EnemyData.csv");

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
                ChaseRange = float.Parse(tokens[10]),
            };

            enemyDataTable[data.Id] = data;
        }
        //Debug.Log($"[DataManager] enemyDataTable : {enemyDataTable.Count}개의 데이터를 로드함");
    }

    public EnemyData GetEnemyData(EnemyId id)
    {
        if (enemyDataTable.TryGetValue(id, out var data) == true)
        {
            return data;
        }

        //Debug.LogWarning($"[DataManager] EnemyId {id}에 해당하는 데이터가 없음");
        return null;
    }

    private void LoadStageData()
    {
        //TextAsset stageText = Resources.Load<TextAsset>("CSV/StageData");
        //string[] lines = stageText.text.Split('\n');
        string[] lines = LoadCsvLines("StageData.csv");

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
                AddCount = int.Parse(tokens[9]),
                BossEnemyId = Enum.Parse<EnemyId>(tokens[10].Trim()),
                RewardType = Enum.Parse<RewardType>(tokens[11].Trim()),
                BossRewardAmount = int.Parse(tokens[12].Trim()),
                BossStatRate = float.Parse(tokens[13].Trim()),
            };

            stageDataTable[stageId] = data;
        }
        //Debug.Log($"[DataManager] stageDataTable : {stageDataTable.Count}개의 데이터를 로드함");
    }

    private void LoadSkillData()
    {
        //TextAsset skillText = Resources.Load<TextAsset>("CSV/SkillData");
        //string[] lines = skillText.text.Split('\n');
        string[] lines = LoadCsvLines("SkillData.csv");

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
                MaxLevels = tokens[13].Trim().Split(';').Select(int.Parse).ToArray(),
                UpgradeCost = int.Parse(tokens[14]),
                UpgradeCostPerLevel = int.Parse(tokens[15]),
                AwakenRequiredCount = tokens[16].Trim().Split(';').Select(int.Parse).ToArray(),
                StatusEffect = Enum.Parse<StatusEffectType>(tokens[17].Trim()),
                StatucChance = float.Parse(tokens[18]),
                HitCount = int.Parse(tokens[19]),
                TargetCount = int.Parse(tokens[20]),
                Range = float.Parse(tokens[21]),
                //isUnlocked = false
            };
            skillDataTable[data.SkillId] = data;
        }
        //Debug.Log($"[DataManager] skillDataTable : {skillDataTable.Count}개의 데이터를 로드함");
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
        //TextAsset goldUpgradeText = Resources.Load<TextAsset>("CSV/GoldUpgradeData");
        //string[] lines = goldUpgradeText.text.Split('\n');
        string[] lines = LoadCsvLines("GoldUpgradeData.csv");

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
        //Debug.Log($"[DataManager] GoldUpgradeData : {skillDataTable.Count}개의 데이터를 로드함");
    }

    public GoldUpgradeData GetGoldUpgradeData(GoldUpgradeType type)
    {
        if (goldUpgradeTable.TryGetValue(type, out GoldUpgradeData data))
        {
            return data;
        }

        Debug.LogWarning($"[DataManager] GoldUpgradeType {type}에 해당하는 데이터가 없음");
        return null;
    }

    private void LoadItemData()
    {
        //TextAsset itemText = Resources.Load<TextAsset>("CSV/ItemData");
        //string[] lines = itemText.text.Split('\n');
        string[] lines = LoadCsvLines("ItemData.csv");

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

        //Debug.Log($"[DataManager] {itemDataTable.Count}의 아이템 데이터가 로드됨");
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
            Debug.LogWarning($"[DataManager] 해당하는 키의 스프라이트를 찾을 수 없음 {key}");
            return null;
        }

    }

    public AudioClip GetAudioClipByKey(string key)
    {
        if (audioDic.TryGetValue(key, out AudioClip audioClip))
        {
            return audioClip;
        }
        else
        {
            Debug.LogWarning($"[DataManager] 해당하는 키의 오디오를 찾을 수 없음 {key}");
            return null;
        }
    }

    private void LoadSummonExpData()
    {
        //TextAsset expText = Resources.Load<TextAsset>("CSV/SummonExpData");
        //string[] lines = expText.text.Split('\n');
        string[] lines = LoadCsvLines("SummonExpData.csv");

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

        //Debug.Log($"[DataManager] 딕셔너리 {summonExpDatas.Count}, 리스트{summonExpDatas.Values.Count}의 데이터를 로드함");
    }

    public int GetSummonMaxExp(SummonSubCategory category, int currentLevel)
    {
        if (summonExpDatas.TryGetValue(category, out var expData))
        {
            //최소 0, 최대 리스트 크기 - 1
            int index = Mathf.Clamp(currentLevel - 1, 0, expData.Count - 1);
            return expData[index];
        }

        return 1000;
    }

    private void LoadSummonGradeProbabilities()
    {
        //TextAsset textAsset = Resources.Load<TextAsset>("CSV/GradeProbabilities");
        //string[] lines = textAsset.text.Split('\n');
        string[] lines = LoadCsvLines("GradeProbabilities.csv");

        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] token = lines[i].Split(',');

            //카테고리, 레벨 파싱
            SummonSubCategory category = Enum.Parse<SummonSubCategory>(token[0]);
            int level = int.Parse(token[1]);

            SummonRateData rateData = new SummonRateData()
            {
                Level = level,
            };

            //등급 파싱
            for (int h = 2; h < token.Length; h++)
            {
                //헤더[2]가 Common
                string grade = headers[h];

                if (Enum.TryParse(grade, out GradeType gradeType))
                {
                    //토큰[2]는 0.5
                    if (float.TryParse(token[h], out float probability))
                    {
                        //확률 딕셔너리 초기화
                        rateData.GradeProbabilities[gradeType] = probability;
                    }
                    else
                    {
                        Debug.LogWarning($"[DataManager] 확률 파싱 실패함, {token[h]}");
                    }
                }
            }

            //딕셔너리에 카테고리의 레벨별 데이터가 존재하지 않는다면
            if (summonRateTable.TryGetValue(category, out var table) == false)
            {
                table = new Dictionary<int, SummonRateData>();
                //카테고리에 대한 내부 딕셔너리 추가
                summonRateTable.Add(category, table);
            }

            //내부 딕셔너리에 레벨별 데이터 추가
            table[level] = rateData;
        }
        //Debug.Log($"[DataManager] 소환 등급 확률 로드 됨, {summonRateTable.Count}");
    }

    private void LoadSummonStageProbabilities()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/StageProbabilities");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("StageProbabilities.csv");

        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            SummonSubCategory category = Enum.Parse<SummonSubCategory>(tokens[0]);
            int level = int.Parse(tokens[1]);

            //카테고리에 해당하는 데이터가 없거나, 카테고리별 레벨에 해당하는 데이터가 없다면
            if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData rateData) == false)
            {
                Debug.LogWarning($"[DataManager] {category} {level} 레벨에 해당하는 소환 레벨 데이터가 없음");
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
                        //1단계부터 시작하려고 +1 함
                        stageProb[j + 1] = p;
                        //Weapon Level 1, Common, { 0.6, 0.4 }
                    }
                    else
                    {
                        Debug.LogWarning($"[DataManager] 잘못된 단계 파싱, {stageTokens[j]}");
                    }
                }

                rateData.StageProbabilities[gradeType] = stageProb;
            }
        }
        //Debug.Log("[DataManager] StageProbabilities 파싱됨");
    }

    public GradeType GetRandomGrade(SummonSubCategory category, int level)
    {
        if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData data) == false)
        {
            Debug.LogWarning($"[DataManager] {category}, {level}에 해당하는 등급 없음");
            return GradeType.Common;
        }

        Dictionary<GradeType, float> gradeProb = data.GradeProbabilities;
        float total = 0f;

        //csv에서 total이 1이 아닐수도 있으므로 따로 계산함
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

        //딕셔너리는 삽입순서를 보장하지 않으므로 따로 만들어 줘야 함.
        //새로운 등급이 추가될시 여기도 추가해줘야 함
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

        Debug.LogWarning($"[DataManager] {category} {level} 확률 계산 실패함, 기본값 반환");
        return GradeType.Common;
    }

    public int GetRandomStage(SummonSubCategory category, int level, GradeType grade)
    {
        if (summonRateTable.TryGetValue(category, out var table) == false || table.TryGetValue(level, out SummonRateData data) == false)
        {
            Debug.LogWarning($"[DataManager] {category}, {level}에 해당하는 정보 없음");
            return 1;
        }

        if (data.StageProbabilities.TryGetValue(grade, out var stageData) == false)
        {
            Debug.LogWarning($"[DataManager] {grade} 등급의 확률 정보 없음");
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

        //딕셔너리 서순 이슈로 키 정렬후 써야 함
        foreach (int stage in stageData.Keys.OrderBy(k => k))
        {
            cumulative += stageData[stage];
            if (rand <= cumulative)
            {
                return stage;
            }
        }

        Debug.LogWarning($"[DataManager] 단계 선택 실패, 기본값 반환");
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
            Debug.LogWarning("[DataManager] 조건에 맞는 아이템 없음");
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
            Debug.LogWarning($"[DataManager] 카테고리가 Skill이 아님 : {category}");
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
            Debug.LogWarning($"[DataManage] {grade}등급의 스킬이 없음");
            return SkillId.None;
        }

        int index = UnityEngine.Random.Range(0, skills.Count);
        return skills[index].SkillId;
    }

    private void LoadSummonRewardData()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/SummonLevelRewardData");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("SummonLevelRewardData.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }
            //Debug.Log($"[라인{i}] {lines[i]}");
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

        //Debug.Log($"[DataManager] 소환 레벨 보상 데이터 로드됨, {summonRewardTable.Count}");
    }

    public SummonRewardData GetRewardData(SummonSubCategory category, int level)
    {
        if (summonRewardTable.TryGetValue(category, out var table) && table.TryGetValue(level, out SummonRewardData data))
        {
            return data;
        }
        Debug.LogWarning($"[DataManager] {category} {level} 레벨에 대한 보상 데이터가 없음");
        return null;
    }

    private void LoadGeneralShopData()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/GeneralShopData");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("GeneralShopData.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            ShopRewardType rewardType = Enum.Parse<ShopRewardType>(tokens[3].Trim());
            ShopPriceType priceType = Enum.Parse<ShopPriceType>(tokens[5].Trim());
            ShopLimitType limitType = Enum.Parse<ShopLimitType>(tokens[8].Trim());

            GeneralShopData generalShopData = new GeneralShopData()
            {
                ShopId = tokens[0].Trim(),
                NameKey = tokens[1].Trim(),
                IconKey = tokens[2].Trim(),
                RewardType = rewardType, //3
                RewardCount = int.Parse(tokens[4].Trim()),
                PriceType = priceType, //5
                PriceAmount = int.Parse(tokens[6].Trim()),
                PurchaseLimit = int.Parse(tokens[7].Trim()),
                LimitType = limitType,
            };

            generalShopDataList.Add(generalShopData);
        }
    }

    private void LoadSkillShopData()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/SkillShopData");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("SkillShopData.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) { continue; }

            string[] tokens = lines[i].Split(',');

            SkillId skillId = Enum.Parse<SkillId>(tokens[3].Trim());
            ShopPriceType priceType = Enum.Parse<ShopPriceType>(tokens[5].Trim());
            ShopLimitType limitType = Enum.Parse<ShopLimitType>(tokens[8].Trim());

            SkillShopData skillShopData = new SkillShopData()
            {
                ShopId = tokens[0].Trim(),
                NameKey = tokens[1].Trim(),
                IconKey = tokens[2].Trim(),
                skillId = skillId,
                RewardCount = int.Parse(tokens[4].Trim()),
                PriceType = priceType,
                PriceAmount = int.Parse(tokens[6].Trim()),
                PurchaseLimit = int.Parse(tokens[7].Trim()),
                LimitType = limitType,
            };

            skillShopDataList.Add(skillShopData);
        }
    }


    private void LoadDungeonData()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/DungeonData");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("DungeonData.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            DungeonType dungeonType = Enum.Parse<DungeonType>(tokens[0].Trim());

            DungeonData dungeonData = new DungeonData()
            {
                DungeonType = dungeonType,
                DungeonSpriteKey = tokens[1].Trim(),
                NameKey = tokens[2].Trim(),
                DescKey = tokens[3].Trim(),
                TicketType = Enum.Parse<PlayerProgressType>(tokens[4].Trim()),
                MainRewardKey = tokens[5].Trim(),
                BGM = tokens[6].Trim(),
            };

            dungeonDataTable.Add(dungeonType, dungeonData);
        }

    }

    public List<GeneralShopData> GetAllGeneralShopDatas()
    {
        return generalShopDataList;
    }

    public List<SkillShopData> GetAllSkillShppDatas()
    {
        return skillShopDataList;
    }

    public DungeonData GetDungeonData(DungeonType type)
    {
        if (dungeonDataTable.TryGetValue(type, out DungeonData data))
        {
            return data;
        }

        return null;
    }

    public Dictionary<DungeonType, DungeonData> GetAllDungeonData()
    {
        return dungeonDataTable;
    }

    private void LoadDungeonLevelData()
    {
        //TextAsset asset = Resources.Load<TextAsset>("CSV/DUngeonLevelData");
        //string[] lines = asset.text.Split('\n');
        string[] lines = LoadCsvLines("DungeonLevelData.csv");


        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            DungeonType type = Enum.Parse<DungeonType>(tokens[0].Trim());

            int level = int.Parse(tokens[1].Trim());

            string[] rewards = tokens[2].Split(';');
            List<PlayerProgressType> currencies = new List<PlayerProgressType>();
            for (int k = 0; k < rewards.Length; k++)
            {
                currencies.Add(Enum.Parse<PlayerProgressType>(rewards[k].Trim()));
            }

            string[] amounts = tokens[3].Split(';');
            List<int> amountList = new List<int>();
            for (int j = 0; j < amounts.Length; j++)
            {
                amountList.Add(int.Parse(amounts[j]));
            }

            string[] spriteKeys = tokens[4].Split(';');
            List<string> spriteKeyList = new List<string>();
            for (int h = 0; h < spriteKeys.Length; h++)
            {
                spriteKeyList.Add(spriteKeys[h].Trim());
            }

            string[] enemyIds = tokens[5].Split(';');
            List<EnemyId> enemyIdList = new List<EnemyId>();
            for (int g = 0; g < enemyIds.Length; g++)
            {
                enemyIdList.Add(Enum.Parse<EnemyId>(enemyIds[g].Trim()));
            }

            DungeonLevelData levelData = new DungeonLevelData()
            {
                DungeonType = type,
                Level = level,
                Currencies = currencies,
                Amounts = amountList,
                SpriteKeys = spriteKeyList,
                EnemyIds = enemyIdList,
                HPRate = float.Parse(tokens[6].Trim()),
                ATKRate = float.Parse(tokens[7].Trim()),
                DEFRate = float.Parse(tokens[8].Trim()),
            };

            if (dungeonLevelDataTable.TryGetValue(type, out var table) == false)
            {
                table = new Dictionary<int, DungeonLevelData>();
                dungeonLevelDataTable.Add(type, table);
            }

            table[level] = levelData;
        }

    }

    public DungeonLevelData GetDungeonLevelData(DungeonType type, int level)
    {
        return dungeonLevelDataTable[type][level];
    }

    public int GetMaxDungeonLevel(DungeonType type)
    {
        return dungeonLevelDataTable[type].Count;
    }

    private void LoadSummonPriceData()
    {
        //TextAsset textAsset = Resources.Load<TextAsset>("CSV/SummonPriceData");
        //string[] lines = textAsset.text.Split('\n');
        string[] lines = LoadCsvLines("SummonPriceData.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            string[] tokens = lines[i].Split(',');

            SummonSubCategory subCategory = Enum.Parse<SummonSubCategory>(tokens[0]);
            int count = int.Parse(tokens[1]);
            int price = int.Parse(tokens[2]);

            SummonPriceData data = new SummonPriceData()
            {
                Category = subCategory,
                Count = count,
                Price = price,
            };

            if (summonPriceDataTable.TryGetValue(subCategory, out Dictionary<int, int> countAndPriceDict) == false)
            {
                countAndPriceDict = new Dictionary<int, int>();
                summonPriceDataTable[subCategory] = countAndPriceDict;
            }

            countAndPriceDict[count] = price;
        }
    }

    public int GetSummonPriceData(SummonSubCategory category, int count)
    {
        if (summonPriceDataTable[category].TryGetValue(count, out int price))
        {
            return price;
        }

        throw new Exception("해당하는 카테고리, 개수에 맞는 가격 정보 없음");
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
    public float ChaseRange;
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
    public EnemyId BossEnemyId;
    public RewardType RewardType;
    public int BossRewardAmount;
    public float BossStatRate;
}

[System.Serializable]
public class SkillData
{
    public SkillId SkillId;
    public string NameKey; //string, string 딕셔너리 만들어서 불러오는 용도
    public string DescKey; //string, string 딕셔너리 만들어서 불러오는 용도
    public string SkillIcon; //추후 DataManager에서 SpriteDictionary로 관리할 예정
    public GradeType Grade; //등급, Common, Uncommon, Rare, Epic, Legendary, Mythical
    public SkillType Type; //Active, Buff, Passive
    public float Cooldown;//스킬 쿨타임, 초 단위
    public float BaseValue;//스킬의 기본 값, 예를 들어 공격력 증가, 체력 회복 등
    public float BaseValueIncrease; //레벨업 시 증가하는 기본 값
    public float BufferDuration; //버프 지속 시간, 초 단위, 버프 스킬에만 적용
    public SkillEffectType EffectType; //스킬 효과 타입, GoldBonus, ExpBonus 등
    public float PassiveValue; //보유 효과
    public float PassiveValuePerLevel; //보유 효과 증가량
    public int[] MaxLevels; //스킬의 최대 레벨
    public int UpgradeCost; //스킬 업그레이드 비용, 골드 등
    public int UpgradeCostPerLevel; //레벨당 업그레이드 비용 증가량
    public int[] AwakenRequiredCount;
    public StatusEffectType StatusEffect;
    public float StatucChance;
    public int HitCount;
    public int TargetCount;
    public float Range;
    //public bool isUnlocked; //여긴 불변데이터 모아놓는 곳인데용, 이게 왜 들어갔을까, PlayerSkillState에서 관리함
    //추후 추가해야 할거 : 스킬 범위

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

    //뽑을 때 등급을 정해주는 확률
    public Dictionary<GradeType, float> GradeProbabilities = new Dictionary<GradeType, float>();

    //등급 안에서 단계별 확률
    //Common, 1단계는 40%, 2단계는 30%
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

public class GeneralShopData
{
    public string ShopId;
    public string NameKey;
    public string IconKey;

    public ShopRewardType RewardType;
    public int RewardCount;

    public ShopPriceType PriceType;
    public int PriceAmount;

    public int PurchaseLimit;
    public ShopLimitType LimitType;
}

public class SkillShopData
{
    public string ShopId;
    public string NameKey;
    public string IconKey;

    public SkillId skillId;
    public int RewardCount;

    public ShopPriceType PriceType;
    public int PriceAmount;

    public int PurchaseLimit;
    public ShopLimitType LimitType;
}

public class DungeonData
{
    public DungeonType DungeonType;
    public string DungeonSpriteKey;
    public string NameKey;
    public string DescKey;
    public PlayerProgressType TicketType;
    public string MainRewardKey;
    public string BGM;
}

public class DungeonLevelData
{
    public DungeonType DungeonType;
    public int Level;
    public List<PlayerProgressType> Currencies;
    public List<int> Amounts;
    public List<string> SpriteKeys;
    public List<EnemyId> EnemyIds;
    public float HPRate;
    public float ATKRate;
    public float DEFRate;
}

public class SummonPriceData
{
    public SummonSubCategory Category;
    public int Count;
    public int Price;
}

[Serializable]
public sealed class HotdataManifest
{
    public string manifestVersion;
    public string generatedAt;
    public HotdataFile[] file;
}

[Serializable]
public sealed class HotdataFile
{
    public string name;
    public string url;
    public string version;
    public string schemaVersion;
    public string sha256;
    public long size;
}