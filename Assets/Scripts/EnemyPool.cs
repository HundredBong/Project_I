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
        //ToArray() : foreach�߿� enemyList.Remove()ȣ��Ǹ� ������ �� ������ �����ϰ� ���纻 ����
        foreach (Enemy enemy in GameManager.Instance.enemyList.ToArray())
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                Return(enemy);
            }
        }
    }
}
