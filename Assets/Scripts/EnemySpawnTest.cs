using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    public GameObject enemyPrefab;

    private void Start()
    {
        ObjectPoolManager.Instance.enemyPool.Preload(enemyPrefab, 5);

        DelayCallManager.Instance.CallLater(1f, () =>
        {
            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(enemyPrefab);
            enemy.transform.position = Vector3.zero;
        });

        DelayCallManager.Instance.CallLater(3f, () =>
        {
            Enemy enemy = FindObjectOfType<Enemy>();
            if (enemy != null)
            {
                ObjectPoolManager.Instance.enemyPool.Return(enemy);
            }
        });
    }
}
