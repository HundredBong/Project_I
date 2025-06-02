using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType type;

    private void OnEnable()
    {
        GameManager.Instance.enemyList.Add(this);
    }
}
