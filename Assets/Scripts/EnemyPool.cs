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
}
