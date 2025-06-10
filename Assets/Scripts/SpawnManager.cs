using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Presets;
using UnityEngine;
using static Cinemachine.Editor.CinemachineLensPresets;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnEnemiesForCurrentStage(int count)
    {
        int stageId = StageManager.Instance.GetCurrentStage();
        StageData stage = DataManager.Instance.stageDataTable[stageId];

        if (stage == null)
        {
            Debug.LogError("[SpawnManager] 현재 스테이지 데이터 없음");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            EnemyId enemyId = stage.Enemies[Random.Range(0, stage.Enemies.Count)];
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyId}");


            if (prefab == null)
            {
                Debug.LogError($"[SpawnManager] 프리팹 로드 실패: Prefabs/Enemies/{enemyId}");
                continue; // null이면 생성하지 않고 다음 반복 진행
            }

            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(prefab);
            enemy.transform.position = GetRandomPos(); // 이 함수는 기존 SpawnManager에 있음
            enemy.transform.rotation = Quaternion.identity;
        }
    }

    private Vector2 GetRandomPos()
    {
        float x = Random.Range(-30.0f, 30.0f);
        float y = Random.Range(0f, 3.5f);
        return new Vector2(x, y);
    }
}