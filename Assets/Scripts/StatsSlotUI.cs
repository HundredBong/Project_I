using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsSlotUI : MonoBehaviour
{
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
    public TextMeshProUGUI statMaxText;
    public Button addButton;

    private PlayerStats stats;
    private StatType statType;

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
    public void Init(PlayerStats stats, StatType type)
    {
        this.stats = stats;

        this.statType = type;

        Refresh();
        addButton.onClick.AddListener(OnClickAdd);

        Debug.Log("[StatsSlotUI] �ʱ�ȭ �Ϸ��");
    }

    public void Refresh()
    {
        //statNameText.text = statType.ToString(); //���� �̸��� enum���� ������ ǥ����. �ӽ÷� �̷��� �ϰ�, ���� CSV���� �о�ͼ� ���� �ٸ��� ����
        //statValueText.text = $"Lv. {stats.GetStat(statType)}"; //���� ������ ������, int�������� ������� �ִ� ü���� ������
        ////������ ���� ���ϴ°� ���� ������� �ִ� ü���� �ƴ� ������ int�� ���� ������ ��������, �� ������ŭ ����� �����̳� �ִ� ü�� ���Ŀ� ���ϴ� �����.
        //statMaxText.text = $"Max Lv. {stats.GetMaxStat(statType)}";
        ////����� ���� 25000���� �س�������, ���� switch expression���� �����丵 �ʿ���.
        try
        {
            statNameText.text = DataManager.Instance.GetLocalizedText($"Stat_{statType}");
            statValueText.text = $"Lv. {stats.GetStat(statType)}";
            statMaxText.text = $"Max Lv. {stats.GetMaxStat(statType)}";
        }
        catch (System.Exception e)
        {
            //�񵿱⿡�� �����ϱ� ��������, ���ȼ��̹����� ���� ������� ���ƿ� �� �����ϵ��� �ؼ� �ذ���
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
