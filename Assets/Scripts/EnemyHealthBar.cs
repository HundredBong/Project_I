using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Image healthFillImage;

    private void Awake()
    {
        if(enemy == null)
        {
            Debug.LogError("[EnemyHealthBar] Enemy 참조가 없음.");
        }
        if (healthFillImage == null)
        {
            Debug.LogError("[EnemyHealthBar] Health Fill Image 참조가 없음.");
        }
    }   

    private void OnEnable()
    {
        enemy.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar();
    }

    private void OnDisable()
    {
        enemy.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        if (enemy.maxHealth <= 0) return;
        float fillAmount = enemy.health / enemy.maxHealth;
        healthFillImage.fillAmount = fillAmount;
    }
}

