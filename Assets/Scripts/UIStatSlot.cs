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
    
    //UIStatPage가 실행시켜 줌
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
            //비동기에서 돌리니까 에러났음, 원래 스레드로 돌아온 뒤 실행하도록 해서 해결함
            Debug.LogError($"[StatSlotUI] Refresh중 예외 발생함, {e.Message}");
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
            Debug.LogWarning("스탯 포인트 0임");
        }
    }

}
