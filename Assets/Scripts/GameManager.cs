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
    }

    private async void Start()
    {
        //statSaver, stats�� null�ε��� �����
        await UniTask.WaitUntil(() => statSaver != null && stats != null && statSaver.gameObject.activeInHierarchy);
        Debug.Log("[GameManager] FirebaseStatSaver, PlayerStats �ʱ�ȭ ��");

        //statSaver �ʱ�ȭ �Ϸ���� �����
        await UniTask.WaitUntil(() => statSaver.isReady);
        Debug.Log("[GameManager] FirebaseStatSaver�� ���̾�̽��� �����");

        //�ҷ����� ����
        statSaver.LoadStatLevels(stats.SetAllLevels);
        Debug.Log("[GameManager] ���� �ҷ����� �����");
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

    [ContextMenu("���� ����")]
    public void SaveStats()
    {
        statSaver.SaveStatLevels(stats.GetAllLevels());
    }

    [ContextMenu("���� �ҷ�����")]
    public void LoadStats()
    {
        statSaver.LoadStatLevels(stats.SetAllLevels);
    }

    #endregion
}
