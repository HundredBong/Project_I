using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("�⺻ ������")]
    public EnemyData data;

    [Header("���� ����")]
    public float health;
    public float maxHealth;
    public bool isDead = false;

    [Header("����")]
    public float expValue = 1f;

    //[HideInInspector] public Enemy prefabReference;

    private void OnEnable()
    {
        if (data == null)
        {
            Debug.LogError("[Enemy] EnemyData�� ������� ����");
            return;
        }

        Initialize();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogError("GameManager �� Null��");
        }

        health = maxHealth;
        isDead = false;
    }

    private void Initialize()
    {
        maxHealth = data.maxHealth;
        health = maxHealth;
        isDead = false;
        expValue = data.expValue;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.player.GetExp(expValue);
        GameManager.Instance.enemyList.Remove(this);
        StageManager.Instance.NotifyKill();

        isDead = true;
        gameObject.SetActive(false);
    }
}
