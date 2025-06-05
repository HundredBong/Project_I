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

    public void Init(PlayerStats stats, StatType type)
    {
        this.stats = stats;
        stats.slotUIs.Add(this);
        this.statType = type;

        Refresh();
        addButton.onClick.AddListener(OnClickAdd);
    }

    public void Refresh()
    {
        //statNameText.text = statType.ToString(); //스탯 이름을 enum에서 가져와 표시함. 임시로 이렇게 하고, 추후 CSV에서 읽어와서 언어별로 다르게 세팅
        //statValueText.text = $"Lv. {stats.GetStat(statType)}"; //현재 스탯을 가져옴, int형식으로 대미지와 최대 체력을 가져옴
        ////하지만 내가 원하는건 현재 대미지나 최대 체력이 아닌 별개의 int형 현재 레벨을 가져오고, 그 레벨만큼 대미지 공식이나 최대 체력 공식에 더하는 방식임.
        //statMaxText.text = $"Max Lv. {stats.GetMaxStat(statType)}";
        ////현재는 전부 25000으로 해놓았지만, 추후 switch expression으로 리팩토링 필요함.

        statNameText.text = DataManager.Instance.statNames[statType].GetLocalizedText();
        statValueText.text = $"Lv. {stats.GetStat(statType)}";
        statMaxText.text = $"Max Lv. {stats.GetMaxStat(statType)}";
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
            Debug.LogWarning("스탯 포인트 0임");
        }
    }

}
