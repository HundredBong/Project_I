using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRankingSlot : PooledUI
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _maxStageText;
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _rankBackgroundImage;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Init(RankingSaveData data, int rank)
    {
        _nameText.text = data.NickName;
        _maxStageText.text = $"{data.MaxClearedStage.ToString("N0")} {DataManager.Instance.GetLocalizedText("UI_Stage")}";
        _levelText.text = $"Lv.{data.Level.ToString("N0")}";

        _rankBackgroundImage.gameObject.SetActive(false);
        _rankText.gameObject.SetActive(false);

        switch (rank)
        {
            case 1:
                _rankBackgroundImage.gameObject.SetActive(true);
                _rankBackgroundImage.sprite = DataManager.Instance.GetSpriteByKey("Rank_1");
                break;
            case 2:
                _rankBackgroundImage.gameObject.SetActive(true);
                _rankBackgroundImage.sprite = DataManager.Instance.GetSpriteByKey("Rank_2");
                break;
            case 3:
                _rankBackgroundImage.gameObject.SetActive(true);
                _rankBackgroundImage.sprite = DataManager.Instance.GetSpriteByKey("Rank_3");
                break;
            default:
                _rankText.gameObject.SetActive(true);
                _rankText.text = rank.ToString();
                break;
        }
    }
}
