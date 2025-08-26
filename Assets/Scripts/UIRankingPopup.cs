using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public class UIRankingPopup : UIPopup
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private Transform _myRankRoot;
    [SerializeField] private GameObject _loadingArea;

    [SerializeField] private TextMeshProUGUI _leaderboardText;
    [SerializeField] private TextMeshProUGUI _loadingText;


    private CancellationTokenSource _cts;
    private DatabaseReference _dbRoot;
    private List<UIRankingSlot> _rankingSlots = new List<UIRankingSlot>();

    protected override void Awake()
    {
        base.Awake();

        _dbRoot = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public override void Open()
    {
        base.Open();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        foreach (var item in _rankingSlots)
        {
            item.gameObject.transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(item);
        }
        _rankingSlots.Clear();

        _loadingArea.SetActive(true);

        _leaderboardText.text = DataManager.Instance.GetLocalizedText("UI_Leaderboard");
        _loadingText.text = DataManager.Instance.GetLocalizedText("Loading");

        LoadAndShowRankingAsync(_cts.Token).Forget();
    }

    public override void Close()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _loadingArea.SetActive(false);

        base.Close();
    }

    private async UniTaskVoid LoadAndShowRankingAsync(CancellationToken ct)
    {
        RankingSaveData myData = await GameManager.Instance.statSaver.LoadRankingData();
        int myStage = myData.MaxClearedStage;

        List<RankingSaveData> rankingSaveDatas = await LoadRankingSaveDatas(ct);

        if (rankingSaveDatas.Count > 0)
        {
            int rank = 1;

            foreach (RankingSaveData item in rankingSaveDatas)
            {
                UIRankingSlot slot = ObjectPoolManager.Instance.uiPool.GetRankingSlot();
                slot.transform.SetParent(_contentRoot.transform, false);
                slot.Init(item, rank);
                Debug.Log($"랭크 : {rank}, 데이터 : {item.NickName}, {item.MaxClearedStage}, {item.Level} ");
                rank++;
                _rankingSlots.Add(slot);
            }
        }

        int myRank = await GetMyRankAsync(myStage, ct);

        UIRankingSlot mySlot = ObjectPoolManager.Instance.uiPool.GetRankingSlot();
        mySlot.transform.SetParent(_myRankRoot.transform, false);
        mySlot.Init(myData, myRank);
        _rankingSlots.Add(mySlot);
        _loadingArea.SetActive(false);
    }

    private async UniTask<List<RankingSaveData>> LoadRankingSaveDatas(CancellationToken ct)
    {
        string path = "leaderboardData";

        DataSnapshot snapshot = await _dbRoot.Child(path).OrderByChild("MaxClearedStage").LimitToLast(100).GetValueAsync().AsUniTask().AttachExternalCancellation(ct);

        List<RankingSaveData> rankingSaveDatas = new List<RankingSaveData>();

        foreach (DataSnapshot child in snapshot.Children)
        {
            RankingSaveData data = new RankingSaveData()
            {
                NickName = child.Child("NickName").Value.ToString(),
                Level = Convert.ToInt32(child.Child("Level").Value),
                MaxClearedStage = Convert.ToInt32(child.Child("MaxClearedStage").Value)
            };

            rankingSaveDatas.Add(data);
        }

        rankingSaveDatas.Reverse();

        return rankingSaveDatas;
    }

    private async UniTask<int> GetMyRankAsync(int myStage, CancellationToken ct)
    {
        string path = "leaderboardData";

        //나보다 점수 높은 사람만 가져오기
        DataSnapshot snapshot = await _dbRoot.Child(path).OrderByChild("MaxClearedStage").StartAt(myStage + 1).GetValueAsync().AsUniTask().AttachExternalCancellation(ct);

        if (snapshot.Exists == false || snapshot.ChildrenCount == 0)
        {
            return 1;
        }

        return (int)snapshot.ChildrenCount + 1;
    }
}
