using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Player player;
    public PlayerStats stats;
    public FirebaseInit firebaseInit;
    public FirebaseStatSaver statSaver;

    [Space(20)]
    public List<Enemy> enemyList = new List<Enemy>();

    [Space(20)]
    public bool firebaseReady = false;
    public bool inventoryReady = false;
    public bool summonReady = false;

    public ShopSummonManager SummonManager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LanguageManager.SetLanguage(System.Enum.Parse<LanguageType>(LocalSetting.LoadLanguage()));
        SummonManager = new ShopSummonManager();

        FindComponent();
    }

    private void FindComponent()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("[GameManager] Player 참조 설정 확인");
            }
        }

        if (stats == null)
        {
            stats = FindObjectOfType<PlayerStats>();
            if (stats == null)
            {
                Debug.LogError("[GameManager] PlayerStats 참조 설정 확인");
            }
        }

        if (statSaver == null)
        {
            statSaver = FindObjectOfType<FirebaseStatSaver>();
            if (statSaver == null)
            {
                Debug.LogError("[GameManager] FirebaseStatSaver 참조 설정 확인");
            }
        }

        if (firebaseInit == null)
        {
            firebaseInit = FindObjectOfType<FirebaseInit>();
            if (firebaseInit == null)
            {
                Debug.LogError("[GameManager] FirebaseInit 참조 설정 확인");
            }
        }
    }

    private async void Start()
    {
        //각종 컴포넌트가 있는지 검사 및 대기
        await UniTask.WaitUntil(() => CheckReadyForLoad());
        Debug.Log("[GameManager] FirebaseStatSaver, PlayerStats 초기화 됨");

        //FirebaseInit에서 초기화 완료까지 대기함
        await UniTask.WaitUntil(() => firebaseReady);
        Debug.Log("[GameManager] FirebaseStatSaver가 파이어베이스에 연결됨");

        //불러오기 실행
        //statSaver.LoadStatLevels(stats.SetAllLevels);
        statSaver.LoadPlayerProgressData(data =>
        {
            stats.LoadProgressSaveData(data);
        });
        Debug.Log("[GameManager] 스탯 불러오기 실행됨");

        //stats.OnStatChanged += () =>
        //{
        //    statSaver.RequestSave(stats.GetAllLevels());
        //};

        statSaver.LoadSkillEquipData(data =>
        {
            SkillManager.Instance.SetEquippedSkills(data.equippedSkills);
            FindObjectOfType<ActiveSkillPanel>().Refresh(data.equippedSkills);
        });

        statSaver.LoadStageData(data =>
        {
            StageManager.Instance.SetStageData(data);
            StageManager.Instance.StartStage();
        });

        statSaver.LoadPlayerSkillData(data =>
        {
            SkillManager.Instance.LoadFrom(data);
        });

        statSaver.LoadInventoryData(data =>
        {
            //게임을 처음 시작하는 경우라면 기본 아이템 지급
            if (data == null || data.InventoryEntries == null || data.InventoryEntries.Count == 0)
            {
                Debug.Log("[GameManager] 인벤토리 데이터 없음, 기본 아이템 지급");
                foreach (var itemData in DataManager.Instance.GetItemData().Values)
                {
                    if (itemData.GradeType == GradeType.Common && itemData.Stage == 1)
                    {
                        InventoryManager.Instance.AddItem(itemData, 1);
                    }
                }
                statSaver.SaveInventoryData(InventoryManager.Instance.GetSaveData());
            }
            else
            {
                InventoryManager.Instance.SetInventoryData(data);
            }
            inventoryReady = true;
        });

        statSaver.LoadSummonProgressData(data => 
        { 
            if (data == null || data.SummonProgressEntries.Count == 0)
            {
                Debug.Log("[GameManager] 소환레벨 데이터 없음, 기본 데이터 초기화");
                foreach (SummonSubCategory category in Enum.GetValues(typeof(SummonSubCategory)))
                {
                    Debug.Log(SummonManager == null ? "SummonManager가 Null임" : "SummonManager가 Null이 아닌데 어째서");
                    SummonManager.AddExp(category, 0);
                    SummonManager.SetLevel(category, 1);
                }

                statSaver.SaveSummonProgress(SummonManager.GetSummonProgressData());
            }
            else
            {
                SummonManager.Init(data);
            }

            summonReady = true;
        });
    }

    private bool CheckReadyForLoad()
    {
        return statSaver != null && stats != null && firebaseInit != null && statSaver.gameObject.activeInHierarchy;
    }

    private void Update()
    {

    }


#if UNITY_EDITOR
    [MenuItem("Tools/Set Language KR")]
    public static void SetLanguageKR()
    {
        LanguageManager.SetLanguage(LanguageType.KR);
    }

    [MenuItem("Tools/Set Language EN")]
    public static void SetLanguageEN()
    {
        LanguageManager.SetLanguage(LanguageType.EN);
    }

    [MenuItem("Tools/Save Stats")]
    public static void SaveStats()
    {
        //Instance.statSaver.SaveStatLevels(Instance.stats.GetAllLevels());
        Instance.statSaver.RequestSave(Instance.stats.GetProgressSaveData());
    }

    [MenuItem("Tools/Load Stats")]
    public static void LoadStats()
    {
        //Instance.statSaver.LoadStatLevels(Instance.stats.SetAllLevels);
        Instance.statSaver.LoadPlayerProgressData(data => Instance.stats.LoadProgressSaveData(data));
    }

    [MenuItem("Tools/Save Skill State")]
    public static void SaveSkillState()
    {
        SkillManager.Instance.AddSkill(SkillId.Lightning, 1);
        Instance.statSaver.SavePlayerSkillData(SkillManager.Instance.BuildSaveData());
    }

    [MenuItem("Tools/Save Inventory Data")]
    public static void SaveInventoryData()
    {
        Instance.statSaver.SaveInventoryData(InventoryManager.Instance.GetSaveData());
    }

#endif
}


