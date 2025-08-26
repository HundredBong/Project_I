using Cysharp.Threading.Tasks;
using Firebase.Database;
using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FirebaseStatSaver : MonoBehaviour
{
    //파이어베이스 실시간 데이터베이스에서 최상위 경로
    private DatabaseReference dbRef;

    //취소 신호를 만들어주는 컨트롤러 객체, async작업을 중간에 취소할 수 있게 해줌. 
    private CancellationTokenSource progressSaveCts;
    private CancellationTokenSource inventorySaveCts;

    private const int MAX_RETRY_COUNT = 5;
    private const int RETRY_DELAY_MS = 300;
    private const float DURATION_THRESHOLD = 0.1f;

    public string Nickname { get; private set; }

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.firebaseReady);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        //Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 됨");
    }

    public void RequestSave(PlayerProgressSaveData data)
    {
        if (progressSaveCts != null)
        {
            progressSaveCts.Cancel();
            progressSaveCts.Dispose();
        }

        progressSaveCts = new CancellationTokenSource();
        DelayAndSave(data, progressSaveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(PlayerProgressSaveData data, CancellationToken token)
    {
        //CancellationToken은 이 작업이 취소되었는지를 체크하는 신호장치
        try
        {
            //2초간 대기하다가 token에서 취소 신호가 오면 중단
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            //SaveStatLevels(statLevels);
            SavePlayerProgressDataAsync(data).Forget();
        }
        catch (OperationCanceledException)
        {
            //중간에 저장 요청이 또 들어오면 무시하기
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
        }
    }

    public async UniTask SavePlayerProgressDataAsync(PlayerProgressSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = GetUserId();
        string path = $"users/{userId}/progress";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 진행 상태 저장 실패, {e}");
        }
    }

    public async UniTask<PlayerProgressSaveData> LoadPlayerProgressDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/progress";

        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[PlayerProgressData] Duration : {duration:F3}");
                if (firstResult == null)
                {
                    firstResult = json;
                }

                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[PlayerProgressData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }


                PlayerProgressSaveData data = string.IsNullOrEmpty(json) ? new PlayerProgressSaveData() : JsonUtility.FromJson<PlayerProgressSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스탯 불러오기 실패: {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"스탯 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveStageDataAsync(StageSaveData data)
    {
        string json = JsonUtility.ToJson(data);

        string userId = GetUserId();
        string path = $"users/{userId}/stage";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스테이지 저장 실패, {e}");

        }
    }

    public async UniTask<StageSaveData> LoadStageDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/stage";
        string firstResult = null;

        for (int i = 0; i < 100; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[StageData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                if (duration < DURATION_THRESHOLD - 0.02f && json == firstResult)
                {
                    Debug.LogWarning($"[StageData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                bool isEmpty = snapshot.Exists == false || json == "null";
                Debug.Log($"Exists : {snapshot.Exists}, json : {json}");

                StageSaveData data = isEmpty ? null : JsonUtility.FromJson<StageSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스테이지 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"스테이지 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveSkillEquipData(SkillEquipSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = GetUserId();
        string path = $"users/{userId}/skillEquip";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스킬 장착 저장 실패, {e}");
        }
    }

    public async UniTask<SkillEquipSaveData> LoadSkillEquipDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/skillEquip";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[SkillEquipSaveData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }

                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[SkillEquipSaveData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SkillEquipSaveData data = string.IsNullOrEmpty(json) ? new SkillEquipSaveData() : JsonUtility.FromJson<SkillEquipSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스킬 장착 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"스킬 장착 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SavePlayerSkillDataAsync(PlayerSkillSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = GetUserId();
        string path = $"users/{userId}/skillState";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스킬 상태 저장 실패, {e}");
        }
    }

    public async UniTask<PlayerSkillSaveData> LoadPlayerSkillDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/skillState";
        string firstResult = null;


        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapShot = await dbRef.Child(path).GetValueAsync();
                string json = snapShot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[PlayerSkillSaveData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[PlayerSkillSaveData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                PlayerSkillSaveData data = string.IsNullOrEmpty(json) ? new PlayerSkillSaveData() : JsonUtility.FromJson<PlayerSkillSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 스킬 상태 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"스킬 상태 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public void RequestSave(InventorySaveData data)
    {
        if (inventorySaveCts != null)
        {
            inventorySaveCts.Cancel();
            inventorySaveCts.Dispose();
        }

        inventorySaveCts = new CancellationTokenSource();
        DelayAndSave(data, inventorySaveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(InventorySaveData data, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            SaveInventoryDataAsync(data).Forget();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
        }
    }

    public async UniTask SaveInventoryDataAsync(InventorySaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = GetUserId();
        string path = $"users/{userId}/InventoryData";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 인벤토리 저장 실패, {e}");
        }
    }

    public async UniTask<InventorySaveData> LoadInventoryDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/InventoryData";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[InventorySaveData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[InventorySaveData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                InventorySaveData data = string.IsNullOrEmpty(json) ? new InventorySaveData() : JsonUtility.FromJson<InventorySaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 인벤토리 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"플레이어 인벤토리 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveSummonProgressAsync(SummonProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = GetUserId();
        string path = $"users/{userId}/SummonProgress";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 소환 레벨 저장 실패, {e}");
        }
    }

    public async UniTask<SummonProgressData> LoadSummonProgressDataAsync()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/SummonProgress";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[SummonProgressData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[SummonProgressData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SummonProgressData data = string.IsNullOrEmpty(json) ? new SummonProgressData() : JsonUtility.FromJson<SummonProgressData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 소환 레벨 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"플레이어 소환 레벨 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SavePurchaseData(ShopPurchaseData data)
    {
        string json = JsonConvert.SerializeObject(data);
        string userId = GetUserId();
        string path = $"users/{userId}/ShopPurchaseData";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 구매 정보 저장 실패, {e}");
        }
    }

    public async UniTask<ShopPurchaseData> LoadPurchaseData()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/ShopPurchaseData";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();

                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[ShopPurchaseData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[ShopPurchaseData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                ShopPurchaseData data = string.IsNullOrEmpty(json) ? new ShopPurchaseData() : JsonConvert.DeserializeObject<ShopPurchaseData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 구매 정보 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"[ShopPurchaseData] 구매 정보 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveDungeonClearedData(DungeonSaveData data)
    {
        string json = JsonConvert.SerializeObject(data);
        string userId = GetUserId();
        string path = $"users/{userId}/DungeonClearedData";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 던전 정보 저장 실패, {e}");
        }
    }

    public async UniTask<DungeonSaveData> LoadDungeonClearedData()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/DungeonClearedData";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();

                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[DungeonSaveData] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[DungeonClearedData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                DungeonSaveData data = string.IsNullOrEmpty(json) ? new DungeonSaveData() : JsonConvert.DeserializeObject<DungeonSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 던전 정보 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"[DungeonClearedData] 던전 정보 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }


    //public async UniTask SaveStageClearIndexAsync(int maxClearedStageId)
    //{
    //    string userId = GetUserId();
    //    string path = $"leaderboardIndex/{userId}";

    //    try
    //    {
    //        await dbRef.Child(path).SetValueAsync(maxClearedStageId);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"[FirebaseStatSaver] 스테이지 클리어 인덱스 저장 실패, {e}");
    //    }
    //}


    //public async UniTask<int> LoadStageClearIndex()
    //{
    //    string userId = GetUserId();
    //    string path = $"leaderboardIndex/{userId}";
    //    string firstResult = null;

    //    for (int i = 0; i < MAX_RETRY_COUNT; i++)
    //    {
    //        float start = Time.realtimeSinceStartup;
    //        try
    //        {
    //            DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
    //            string json = snapshot.GetRawJsonValue();

    //            float duration = Time.realtimeSinceStartup - start;

    //            if (firstResult == null)
    //            {
    //                firstResult = json;
    //            }
    //            else if (duration < DURATION_THRESHOLD && firstResult == json)
    //            {
    //                Debug.LogWarning($"[StageClearIndex] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
    //                await UniTask.Delay(RETRY_DELAY_MS);
    //                continue;
    //            }
    //            int clearIndex = string.IsNullOrEmpty(json) ? 0 : int.Parse(json);
    //            await UniTask.SwitchToMainThread();
    //            return clearIndex;
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 던전 정보 불러오기 실패, {e}");
    //            await UniTask.Delay(RETRY_DELAY_MS);
    //        }
    //    }

    //    await UniTask.SwitchToMainThread();
    //    throw new Exception($"[StageClearIndex] 던전 정보 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    //}

    public async UniTask SaveNickname(string nickname)
    {
        string userId = GetUserId();
        string path = $"users/{userId}/Nickname";

        try
        {
            await dbRef.Child(path).SetValueAsync(nickname);
            Nickname = nickname;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 닉네임 정보 저장 실패, {e}");
        }
    }

    public async UniTask<string> LoadNickname()
    {
        string userId = GetUserId();
        string path = $"users/{userId}/Nickname";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string loadedName = snapshot.Value?.ToString() ?? "Default"; //Value에서 NRE떠서 교체함
                float duration = Time.realtimeSinceStartup - start;
                Debug.Log($"[Nickname] Duration : {duration:F3}");

                if (firstResult == null)
                {
                    firstResult = loadedName;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == loadedName)
                {
                    Debug.LogWarning($"[LoadNickname] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                string nickname = string.IsNullOrEmpty(loadedName) ? "Default" : loadedName;
                await UniTask.SwitchToMainThread();
                Nickname = nickname;
                return nickname;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 닉네임 정보 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"[LoadNickname] 닉네임 정보 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveRanking(RankingSaveData saveData)
    {
        string uid = GetUserId();
        string path = $"leaderboardData/{uid}";
        string json = JsonUtility.ToJson(saveData);

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 랭킹 데이터 저장 실패, {e}");
        }
    }

    public async UniTask<RankingSaveData> LoadRankingData()
    {
        string userId = GetUserId();
        string path = $"leaderboardData/{userId}";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();

                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[RankingSaveData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                RankingSaveData data = string.IsNullOrEmpty(json) ? new RankingSaveData() : JsonConvert.DeserializeObject<RankingSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 랭킹 정보 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"[RankingSaveData] 랭킹 정보 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask<long> GetServerNowMsAsync()
    {
        //서버 시간 저장 및 불러오기

        string uid = GetUserId();
        string path = $"users/{uid}/timestamp";

        await dbRef.Child(path).SetValueAsync(ServerValue.Timestamp);

        DataSnapshot snap = await dbRef.Child(path).GetValueAsync();

        if (snap.Exists == true && long.TryParse(snap.Value.ToString(), out long ms))
        {
            return ms;
        }

        throw new Exception("서버 시간 가져오기 실패함");
    }

    public async UniTask<long> GetLastActiveMsAsync()
    {
        //마지막 활동 시간 불러오기

        string uid = GetUserId();
        string path = $"users/{uid}/lastActiveMs";
        DataSnapshot snap = await dbRef.Child(path).GetValueAsync();

        if (snap.Exists == true && long.TryParse(snap.Value.ToString(), out long ms))
        {
            return ms;
        }

        return 0L;
    }

    public async UniTask SetLastActiveNowAsync()
    {
        //현재 시간을 서버에 저장

        string uid = GetUserId();
        string path = $"users/{uid}/lastActiveMs";

        await dbRef.Child(path).SetValueAsync(ServerValue.Timestamp);
    }

    public async UniTask SaveLastActiveMsAsync(long nowMs)
    {
        //외부에서 ms를 받아서 시간을 서버에 저장 

        string uid = GetUserId();
        string path = $"users/{uid}/lastActiveMs";

        await dbRef.Child(path).SetValueAsync(nowMs);
    }

    private string GetUserId()
    {

#if UNITY_ANDROID || UNITY_IOS
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            throw new Exception("[FirebaseStatSaver] 현재 로그인된 유저가 없음");
        }
        return user?.UserId;
#else
        return "test_user"; //에디터에서는 테스트 유저 ID 사용
#endif
    }



}

[System.Serializable]
public class StageSaveData
{
    public int CurrentStageId;
    public int MaxClearedStageId;
    public bool[] BossChallengable;
    public bool[] BossDefeated;

    public StageSaveData()
    {
        CurrentStageId = 1;
        MaxClearedStageId = 0;
        BossChallengable = new bool[DataManager.Instance.stageDataTable.Count];
        BossDefeated = new bool[DataManager.Instance.stageDataTable.Count];
    }
}

[System.Serializable]
public class SkillEquipSaveData
{
    public SkillId[] equippedSkills = new SkillId[6];
}

[System.Serializable]
public class SkillStateSaveData
{
    public SkillId skillId; //enum 필드는 괜찮음
    public int level;
    public int ownedCount;
    public int awakenLevel;
}

[System.Serializable]
public class PlayerSkillSaveData
{
    public List<SkillStateSaveData> skillStates = new List<SkillStateSaveData>();
}

[System.Serializable]
public class StatLevelEntry
{
    public StatUpgradeType StatUpgradeType;
    public int Level;
}

[System.Serializable]
public class ProgressEntry
{
    public PlayerProgressType PlayerProgressType;
    public float Value;
}

[System.Serializable]
public class GoldLevelEntry
{
    public GoldUpgradeType GoldUpgradeType;
    public int Level;
}

[System.Serializable]
public class PlayerProgressSaveData
{
    public List<ProgressEntry> progressValues = new();
    public List<StatLevelEntry> statLevels = new();
    public List<GoldLevelEntry> goldUpgradeLevels = new();
}

[System.Serializable]
public class InventoryEntry
{
    public int Id;
    public int Level;
    public int Count;
    public bool IsEquipped;
    public bool IsUnlocked;
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryEntry> InventoryEntries = new List<InventoryEntry>();
}


[System.Serializable]
public class SummonProgressEntry
{
    public SummonSubCategory Category;
    public int Level;
    public int Exp;
}

[System.Serializable]
public class SummonRewardClaimEntry
{
    public SummonSubCategory Category;
    public List<int> Levels = new List<int>();
}

[System.Serializable]
public class SummonProgressData
{
    public List<SummonProgressEntry> SummonProgressEntries = new List<SummonProgressEntry>();
    public List<SummonRewardClaimEntry> SummonRewardEntries = new List<SummonRewardClaimEntry>();
}

[System.Serializable]
public class ShopPurchaseEntry
{
    public int PurchaseCount; //누적 총합

    public int PeriodCount; //현재 기간 구매 수
    public string WindowKey; //기간 키

    public long LastPurchased; //마지막 구매 시간, ISO 8601 형식으로 저장 이였다가 ms로 바뀜
}

[System.Serializable]
public class ShopPurchaseData
{
    public Dictionary<string, ShopPurchaseEntry> PurchaseEntries = new Dictionary<string, ShopPurchaseEntry>();
}

[System.Serializable]
public class DungeonSaveData
{
    public Dictionary<DungeonType, int> DungeonClearedData = new Dictionary<DungeonType, int>();
}

[System.Serializable]
public class RankingSaveData
{
    public string NickName;
    public int Level;
    public int MaxClearedStage;
}