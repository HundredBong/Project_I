using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("스탯")]
    public float health;
    public float speed;
    public float maxHealth;
    public float maxSpeed;
    public float damage;
    public float critical;
    public float teleportDistance;


    //Private Field
    private Enemy targetEnemy;
    private float currentDistance;

    private void Update()
    {
        CheckDistance();

        if (currentDistance < teleportDistance)
        {
            Vector3 targetDir = (targetEnemy.transform.position - transform.position).normalized;
            transform.position += targetDir;
        }
    }

    private void CheckDistance()
    {
        //가장 가까운 Enemy객체 찾기
        float targetDistance = float.MaxValue;

        foreach (Enemy enemy in GameManager.Instance.enemyList)
        {
            if (enemy == null) { continue; }

            float distance = Mathf.Abs(Vector3.Distance(enemy.transform.position, transform.position));

            if (distance < targetDistance)
            {
                targetDistance = distance;
                currentDistance = distance;
                targetEnemy = enemy;
                Debug.Log(targetEnemy.name);
            }
        }
    }
}
