using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    #endregion
}
