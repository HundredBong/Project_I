using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UIRankingPopup : UIPopup
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _loadingArea;

    private CancellationTokenSource _cts;
    private DatabaseReference _dbRoot;
    private List<UIRankingSlot> _rankingItems = new List<UIRankingSlot>();

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

        foreach (var item in _rankingItems)
        {
            item.gameObject.transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(item);
        }
        _rankingItems.Clear();

        _loadingArea.SetActive(true);

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
        try
        {
            int myScore = await GameManager.Instance.statSaver.LoadStageClearIndex();
            List<(string uid, int score)> top = await FetchTop100Async(ct);

            if (top.Count == 0)
            {
                _loadingArea.SetActive(false);
                ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_NoRanking");
                Debug.LogWarning("No ranking data found.");
                return;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading ranking: {ex.Message}");
        }
    }

    private async UniTask<List<(string uid, int score)>> FetchTop100Async(CancellationToken ct)
    {
        string path = "leaderboardIndex";

        //userId의 점수를 기준으로 정렬
        //오름차순 정렬후 마지막 100개 (점수가 높은 100개)만 가져옴
        //이 경로의 데이터를 읽어서 DataSnapshot 객체로 가져옴
        //AsUniTask()로 Task -> UniTask로 변환
        DataSnapshot snapshot = await _dbRoot.Child(path).OrderByValue().LimitToLast(100).
            GetValueAsync().AsUniTask().AttachExternalCancellation(ct);

        //튜플
        List<(string uid, int score)> list = new List<(string uid, int score)>();

        //DataSnapshot이 비어있지 않은지 확인
        if (snapshot.Exists == false)
        {
            return list;
        }


        foreach (DataSnapshot data in snapshot.Children)
        {
            //uid, score 추가
            list.Add((data.Key, Convert.ToInt32(data.Value)));
        }

        return list;
    }

    private async UniTask<int> ComputeMyRankAsync(int myStage, CancellationToken ct)
    {
        string path = "leaderboardIndex";

        //나보다 점수 높은 사람만 가져오기
        DataSnapshot snapshot = await _dbRoot.Child(path).OrderByValue().StartAt(myStage + 1).GetValueAsync().AsUniTask().AttachExternalCancellation(ct);

        if (snapshot.Exists == false)
        {
            return 0;
        }

        //내 순위 만들기
        int higher = snapshot.Children.Count();
        return higher + 1;
    }
}
