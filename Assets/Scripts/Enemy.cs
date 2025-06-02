using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType type;

    public float health;
    public float maxHealth;
    public bool isDead;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogError("GameManager ∞° Null¿”");
        }

        health = maxHealth;
        isDead = false;
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
        gameObject.SetActive(false);
        GameManager.Instance.enemyList.Remove(this);
        isDead = true;
    }
}
