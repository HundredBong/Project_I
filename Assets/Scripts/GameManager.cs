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
    public FirebaseStatSaver statSaver;

    [Space(20)]
    public List<Enemy> enemyList = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //------------------------------------------------------------

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
    }

    private async void Start()
    {
        //statSaver, stats가 null인동안 대기함
        await UniTask.WaitUntil(() => statSaver != null && stats != null && statSaver.gameObject.activeInHierarchy);
        Debug.Log("[GameManager] FirebaseStatSaver, PlayerStats 초기화 됨");

        //statSaver 초기화 완료까지 대기함
        await UniTask.WaitUntil(() => statSaver.isReady);
        Debug.Log("[GameManager] FirebaseStatSaver가 파이어베이스에 연결됨");

        //불러오기 실행
        statSaver.LoadStatLevels(stats.SetAllLevels);
        Debug.Log("[GameManager] 스탯 불러오기 실행됨");
    }

    #region ContextMenu
    [ContextMenu("KR")]
    public void TestKR()
    {
        LanguageManager.SetLanguage(LanguageType.KR);
    }

    [ContextMenu("EN")]
    public void TestEN()
    {
        LanguageManager.SetLanguage(LanguageType.EN);
    }

    [ContextMenu("스탯 저장")]
    public void SaveStats()
    {
        statSaver.SaveStatLevels(stats.GetAllLevels());
    }

    [ContextMenu("스탯 불러오기")]
    public void LoadStats()
    {
        statSaver.LoadStatLevels(stats.SetAllLevels);
    }

    #endregion
}
