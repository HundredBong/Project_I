using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIToastReward : PooledUI
{
    private CanvasGroup _cg;
    private TextMeshProUGUI _messageText;
    private const float MOVE_TIME = 1.5f;

    [SerializeField] private TextMeshProUGUI _rewardTitle;
    [SerializeField] private Image[] _rewardIcons;
    [SerializeField] private TextMeshProUGUI[] _rewardCounts;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
    }

    public void Init(Sprite[] rewardImages, int[] rewardCounts)
    {
        DontDestroyOnLoad(gameObject);

        if (rewardImages.Length != rewardCounts.Length)
        {
            Debug.LogWarning("[UIToastReward] 인자 갯수가 일치하지 않음");
            return;
        }

        _rewardTitle.text = DataManager.Instance.GetLocalizedText("UI_ToastReward"); //단발성 팝업이므로 이벤트 등록하지 않음

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastReward] ToastRoot가 없음");
        }
        else
        {
            transform.SetParent(root);
        }

        foreach (var image in _rewardIcons)
        {
            image.gameObject.SetActive(false);
        }

        foreach (var text in _rewardCounts)
        {
            text.gameObject.SetActive(false);
        }

        int count = Mathf.Min(rewardImages.Length, _rewardIcons.Length, _rewardCounts.Length);

        for (int i = 0; i < count; i++)
        {
            _rewardIcons[i].gameObject.SetActive(true);
            _rewardIcons[i].sprite = rewardImages[i];

            bool isZero = rewardCounts[i] == 0;

            //0이 아닐 경우에만 오브젝트 활성화
            _rewardCounts[i].gameObject.SetActive(isZero == false);
            _rewardCounts[i].text = rewardCounts[i].ToString();
        }

        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }
    public void Init(string rewardIconKey, int rewardCount)
    {
        DontDestroyOnLoad(gameObject);

        _rewardTitle.text = DataManager.Instance.GetLocalizedText("UI_ToastReward");

        foreach (var image in _rewardIcons)
        {
            image.gameObject.SetActive(false);
        }

        foreach (var text in _rewardCounts)
        {
            text.gameObject.SetActive(false);
        }

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastReward] ToastRoot가 없음");
        }
        else
        {
            transform.SetParent(root);
        }

        //이미 있는게 다시 Init되지는 않음. 애초에 사용중이고 풀에 들어가있어야 Init이 실행될거임.
        //그럼 이전에 Init한게 현재 Init에 영향을 주지는 않을거임. 아마,

        _rewardIcons[0].sprite = DataManager.Instance.GetSpriteByKey(rewardIconKey);
        _rewardIcons[0].gameObject.SetActive(true);

        bool isZero = rewardCount == 0;

        _rewardCounts[0].gameObject.SetActive(isZero == false);
        _rewardCounts[0].text = rewardCount.ToString();

        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () =>
        {
            transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(this);
        });
    }
}
