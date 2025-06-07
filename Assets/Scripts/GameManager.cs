using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        statSaver.LoadStatLevels(stats.SetAllLevels);
        Debug.Log("[GameManager] ���� �ҷ����� �����");

        stats.OnStatChanged += () =>
        {
            statSaver.RequestSave(stats.GetAllLevels());
        };
    }

    private bool CheckReadyForLoad()
    {
        return statSaver != null && stats != null && firebaseInit != null && statSaver.gameObject.activeInHierarchy;
    }

    #region ContextMenu
    //[ContextMenu("KR")]
    //public void TestKR()
    //{
    //    LanguageManager.SetLanguage(LanguageType.KR);
    //}

    //[ContextMenu("EN")]
    //public void TestEN()
    //{
    //    LanguageManager.SetLanguage(LanguageType.EN);
    //}

    //[ContextMenu("���� ����")]
    //public void SaveStats()
    //{
    //    statSaver.SaveStatLevels(stats.GetAllLevels());
    //}

    //[ContextMenu("���� �ҷ�����")]
    //public void LoadStats()
    //{
    //    statSaver.LoadStatLevels(stats.SetAllLevels);
    //}

    #endregion

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
        Instance.statSaver.SaveStatLevels(Instance.stats.GetAllLevels());
    }

    [MenuItem("Tools/Load Stats")]
    public static void LoadStats()
    {
        Instance.statSaver.LoadStatLevels(Instance.stats.SetAllLevels);
    }
#endif
}


