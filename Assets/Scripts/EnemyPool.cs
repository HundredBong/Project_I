using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : GenericPoolManager<Enemy>
{
    public override void Preload(GameObject prefab, int count)
    {
        base.Preload(prefab, count); 

        Stack<Enemy> stack;

        if (pool.TryGetValue(prefab, out stack))
        {
            foreach (var enemy in stack)
            {
                enemy.isDead = true;
            }
        }
    }

    public void ReturnAllEnemies()
    {
        //ToArray() : foreach중에 enemyList.Remove()호출되면 오류날 수 있으니 안전하게 복사본 돌림
        foreach (Enemy enemy in GameManager.Instance.enemyList.ToArray())
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                Return(enemy);
            }
        }
    }
}
