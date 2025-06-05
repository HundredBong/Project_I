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

    public void Init(PlayerStats stats, StatType type)
    {
        this.stats = stats;
        this.statType = type;

        Refresh();
        addButton.onClick.AddListener(OnClickAdd);
    }

    public void Refresh()
    {
        statNameText.text = statType.ToString(); //���� �̸��� enum���� ������ ǥ����. �ӽ÷� �̷��� �ϰ�, ���� CSV���� �о�ͼ� ���� �ٸ��� ����
        statValueText.text = $"Lv. {stats.GetStat(statType)}"; //���� ������ ������, int�������� ������� �ִ� ü���� ������
        //������ ���� ���ϴ°� ���� ������� �ִ� ü���� �ƴ� ������ int�� ���� ������ ��������, �� ������ŭ ����� �����̳� �ִ� ü�� ���Ŀ� ���ϴ� �����.
        statMaxText.text = $"Max Lv. {stats.GetStatMax(statType)}";
        //����� ���� 25000���� �س�������, ���� switch expression���� �����丵 �ʿ���.
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
            Debug.LogWarning("���� ����Ʈ�� ������");
        }
    }

}
