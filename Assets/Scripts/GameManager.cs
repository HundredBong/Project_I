using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
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
    public bool loadSceneReady = false;

    public ShopManager ShopManager { get; private set; }

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
        ShopManager = new ShopManager();

        FindComponent();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    private void FindComponent()
    {
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
        if (stageData == null || stageData.CurrentStageId == 0 || stageData.MaxClearedStageId == 0)
        {
            Debug.Log("새로운 스테이지 데이터 작성");
            stageData = new StageSaveData();
            await statSaver.SaveStageDataAsync(stageData);
        }
        StageManager.Instance.SetStageData(stageData);

        //StageManager 
        //StageManager.Instance.StartStage();

        PlayerSkillSaveData skillState = await statSaver.LoadPlayerSkillDataAsync();
        SkillManager.Instance.LoadFrom(skillState);

        SkillEquipSaveData equipData = await statSaver.LoadSkillEquipDataAsync();
        SkillManager.Instance.SetEquippedSkills(equipData.equippedSkills);

        //ActiveSkillPanel의 Start에서 Refresh하도록 함
        //FindObjectOfType<ActiveSkillPanel>().Refresh(SkillManager.Instance.GetEquippedSkills());

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
        InventoryManager.Instance.SetInventoryData(inventoryData);
        inventoryReady = true;

        var summonProgressData = await statSaver.LoadSummonProgressDataAsync();
        if (summonProgressData == null || summonProgressData.SummonProgressEntries.Count == 0)
        {
            foreach (SummonSubCategory category in Enum.GetValues(typeof(SummonSubCategory)))
            {
                ShopManager.AddExp(category, 0);
                ShopManager.SetLevel(category, 1);
            }

            await statSaver.SaveSummonProgressAsync(ShopManager.BuildSummonProgressData());
        }
        ShopManager.InitSummonProgressData(summonProgressData);

        var purchaseProgressData = await statSaver.LoadPurchaseData();

        if (purchaseProgressData == null || purchaseProgressData.PurchaseEntries.Count == 0)
        {
            purchaseProgressData.PurchaseEntries["Dummy"] = new ShopPurchaseEntry
            {
                PurchaseCount = 1,
                LastPurchased = DateTime.UtcNow.ToString("o")
            };
            await statSaver.SavePurchaseData(purchaseProgressData);
        }
        ShopManager.InitPurchaseData(purchaseProgressData);
        summonReady = true;

        var dungeonClearedData = await statSaver.LoadDungeonClearedData();

        if (dungeonClearedData == null)
        {
            dungeonClearedData = new DungeonSaveData();
        }

        //게임을 처음 시작한 경우
        if (dungeonClearedData.DungeonClearedData.Count == 0)
        {
            //모든 레벨을 1로 초기화
            foreach (DungeonType item in Enum.GetValues(typeof(DungeonType)))
            {
                dungeonClearedData.DungeonClearedData[item] = 1;
            }

            statSaver.SaveDungeonClearedData(dungeonClearedData).Forget();
        }

        StageManager.Instance.SetDungeonData(dungeonClearedData);

        //loadSceneReady = true; <- AdManager에서 처리

        AdManager.Instance.ShowLaunchInterstitialOnce();
    }

    private bool CheckReadyForLoad()
    {
        return statSaver != null && stats != null && firebaseInit != null && statSaver.gameObject.activeInHierarchy;
    }

    public void RegistPlayer(Player player)
    {
        this.player = player;
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

    [MenuItem("Tools/Go To Main Scene (Game)")]
    public static void GoToMainScene()
    {
        LoadingSceneController.LoadScene("0_MainScene");
    }

    [MenuItem("Tools/Go To Stage Scene (Game)")]
    public static void GoToStageScene()
    {
        LoadingSceneController.LoadScene("1_StageScene");
    }

    [MenuItem("Tools/Go To Enhance Dungeon Scene (Game)")]
    public static void GoToEnhanceDungeonScene()
    {
        LoadingSceneController.LoadScene("2_EnhanceDungeonScene");
    }

    [MenuItem("Tools/Go To Skill Dungeon Scene (Game)")]
    public static void GoToSkillDungeonScene()
    {
        LoadingSceneController.LoadScene("3_SkillDungeonScene");
    }

    [MenuItem("Scenes/Go To Stage Scene (Editor)")]
    private static void OpenStageScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/1_StageScene.unity");
        }
    }

    [MenuItem("Scenes/Go To Main Menu (Editor)")]
    private static void OpenMainMenuScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/0_MainScene.unity");
        }
    }

    [MenuItem("Scenes/Go To Enhance Dungeon (Editor)")]
    private static void OpenEnhanceDungeonScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/2_EnhanceDungeonScene.unity");
        }
    }

    [MenuItem("Scenes/Go To Skill Dungeon (Editor)")]
    private static void OpenSkillDungeonScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/3_SkillDungeonScene.unity");
        }
    }

#endif
}


