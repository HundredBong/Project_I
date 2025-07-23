using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatSlot : MonoBehaviour
{
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
    public TextMeshProUGUI statMaxText;
    public Button addButton;

    private PlayerStats stats;
    private StatUpgradeType statType;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;

        if (stats != null)
        {
            Refresh();
        }

    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }
    
    //UIStatPage�� ������� ��
    public void Init(PlayerStats stats, StatUpgradeType type)
    {
        this.stats = stats;

        this.statType = type;

        Refresh();
        addButton.onClick.AddListener(OnClickAdd);
    }

    public void Refresh()
    {
        try
        {
            statNameText.text = DataManager.Instance.GetLocalizedText($"Stat_{statType}");
            statValueText.text = $"Lv. {stats.GetStat(statType)}";
            statMaxText.text = $"Max Lv. {stats.GetMaxStat(statType)}";
        }
        catch (System.Exception e)
        {
            //�񵿱⿡�� �����ϱ� ��������, ���� ������� ���ƿ� �� �����ϵ��� �ؼ� �ذ���
            Debug.LogError($"[StatSlotUI] Refresh�� ���� �߻���, {e.Message}");
        }
    }

    private void OnClickAdd()
    {
        if (stats.statPoint > 0)
        {
            stats.AddStat(statType, StatUpgradeAmount.statSlotAmount);
            Refresh();
        }
        else
        {
            Debug.LogWarning("���� ����Ʈ 0��");
        }
    }

}
