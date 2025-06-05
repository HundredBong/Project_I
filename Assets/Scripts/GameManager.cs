using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Player player;
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
        }
    }

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
}
