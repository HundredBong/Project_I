using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStatusBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentHealthText;
    [SerializeField] private TextMeshProUGUI currentExpText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image expFillImage;
    [SerializeField] private TextMeshProUGUI _nicknameText;

    private PlayerStats _stats;

    private void Awake()
    {
        _stats = GameManager.Instance.stats != null ? GameManager.Instance.stats : null;

        if (_stats == null)
        {
            Debug.LogWarning("[UIStatusBar] PlayerStats 참조 확인");
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.stats.OnStatusChanged += Refresh;
    }

    private void OnDisable()
    {
        GameManager.Instance.stats.OnStatusChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
        _nicknameText.text = GameManager.Instance.statSaver.Nickname;
    }

    private void Refresh()
    {
        float health = Mathf.Max(0, _stats.health);
        currentHealthText.text = NumberFormatter.FormatNumber(health);
        float exp = _stats.currentExp / _stats.maxExp * 100;
        currentExpText.text = $"{exp:F2}%";
        currentLevelText.text = $"Lv.{_stats.level}";

        healthFillImage.fillAmount = Mathf.Min(1f, _stats.health / _stats.maxHealth);
        expFillImage.fillAmount = Mathf.Min(1f, _stats.currentExp / _stats.maxExp);
    }
}
