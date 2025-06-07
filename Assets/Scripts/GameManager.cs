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
        statSaver.LoadStatLevels(stats.SetAllLevels);
        Debug.Log("[GameManager] 스탯 불러오기 실행됨");

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

    //[ContextMenu("스탯 저장")]
    //public void SaveStats()
    //{
    //    statSaver.SaveStatLevels(stats.GetAllLevels());
    //}

    //[ContextMenu("스탯 불러오기")]
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


