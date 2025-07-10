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
                Debug.LogError("[GameManager] Player ���� ���� Ȯ��");
            }
        }

        if (stats == null)
        {
            stats = FindObjectOfType<PlayerStats>();
            if (stats == null)
            {
                Debug.LogError("[GameManager] PlayerStats ���� ���� Ȯ��");
            }
        }

        if (statSaver == null)
        {
            statSaver = FindObjectOfType<FirebaseStatSaver>();
            if (statSaver == null)
            {
                Debug.LogError("[GameManager] FirebaseStatSaver ���� ���� Ȯ��");
            }
        }

        if (firebaseInit == null)
        {
            firebaseInit = FindObjectOfType<FirebaseInit>();
            if (firebaseInit == null)
            {
                Debug.LogError("[GameManager] FirebaseInit ���� ���� Ȯ��");
            }
        }
    }

    private async void Start()
    {
        //���� ������Ʈ�� �ִ��� �˻� �� ���
        await UniTask.WaitUntil(() => CheckReadyForLoad());
        Debug.Log("[GameManager] FirebaseStatSaver, PlayerStats �ʱ�ȭ ��");

        //FirebaseInit���� �ʱ�ȭ �Ϸ���� �����
        await UniTask.WaitUntil(() => firebaseReady);
        Debug.Log("[GameManager] FirebaseStatSaver�� ���̾�̽��� �����");

        //�ҷ����� ����
        //statSaver.LoadStatLevels(stats.SetAllLevels);
        statSaver.LoadPlayerProgressData(data =>
        {
            stats.LoadProgressSaveData(data);
        });
        Debug.Log("[GameManager] ���� �ҷ����� �����");

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
            //������ ó�� �����ϴ� ����� �⺻ ������ ����
            if (data == null || data.InventoryEntries == null || data.InventoryEntries.Count == 0)
            {
                Debug.Log("[GameManager] �κ��丮 ������ ����, �⺻ ������ ����");
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
                Debug.Log("[GameManager] ��ȯ���� ������ ����, �⺻ ������ �ʱ�ȭ");
                foreach (SummonSubCategory category in Enum.GetValues(typeof(SummonSubCategory)))
                {
                    Debug.Log(SummonManager == null ? "SummonManager�� Null��" : "SummonManager�� Null�� �ƴѵ� ��°��");
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


