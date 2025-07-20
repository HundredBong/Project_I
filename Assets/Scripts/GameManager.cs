using Cysharp.Threading.Tasks;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.U2D;
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

        //FirebaseInit에서 초기화 완료까지 대기함
        await UniTask.WaitUntil(() => firebaseReady);

        //불러오기 실행
        PlayerProgressSaveData playerProgress = await statSaver.LoadPlayerProgressDataAsync();
        stats.InitializeFromProgressData(playerProgress);

        StageSaveData stageData = await statSaver.LoadStageDataAsync();
        StageManager.Instance.SetStageData(stageData);
        StageManager.Instance.StartStage();

        PlayerSkillSaveData skillState = await statSaver.LoadPlayerSkillDataAsync();
        SkillManager.Instance.LoadFrom(skillState);

        SkillEquipSaveData equipData = await statSaver.LoadSkillEquipDataAsync();
        SkillManager.Instance.SetEquippedSkills(equipData.equippedSkills);
        FindObjectOfType<ActiveSkillPanel>().Refresh(SkillManager.Instance.GetEquippedSkills());

        InventorySaveData inventoryData = await statSaver.LoadInventoryDataAsync();
        if (inventoryData == null || inventoryData.InventoryEntries == null || inventoryData.InventoryEntries.Count == 0)
        {
            foreach (var itemData in DataManager.Instance.GetItemData().Values)
            {
                if (itemData.GradeType == GradeType.Common && itemData.Stage == 1)
                {
                    InventoryManager.Instance.AddItem(itemData, 1);
                }
            }
            await statSaver.SaveInventoryDataAsync(InventoryManager.Instance.BuildSaveData());
        }
        inventoryReady = true;

        var summonProgressData = await statSaver.LoadSummonProgressDataAsync();
        if (summonProgressData == null || summonProgressData.SummonProgressEntries.Count == 0)
        {
            foreach (SummonSubCategory category in Enum.GetValues(typeof(SummonSubCategory)))
            {
                SummonManager.AddExp(category, 0);
                SummonManager.SetLevel(category, 1);
            }

            await statSaver.SaveSummonProgressAsync(SummonManager.BuildSummonProgressData());
        }
        SummonManager.Init(summonProgressData);
        summonReady = true;
    }

    private bool CheckReadyForLoad()
    {
        return statSaver != null && stats != null && firebaseInit != null && statSaver.gameObject.activeInHierarchy;
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

#endif
}


