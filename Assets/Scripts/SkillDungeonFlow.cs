using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDungeonFlow : IStageFlow
{
    private StageManager _manager;
    private DungeonLevelData _levelData;
    private int _killCount = 0;
    private int _requiredCount = 50; //스테이지 클리어에 필요한 킬수 및 스폰할 몬스터 수

    private const float FADE_DURATION = 2f;
    private bool _isCleared = false; //스테이지 클리어 여부

    public SkillDungeonFlow(StageManager manager)
    {
        _manager = manager;

        _levelData = DataManager.Instance.GetDungeonLevelData(DungeonType.SkillDungeon, StageManager.Instance.skillDungeonLevel);
    }

    public void Start()
    {
        _isCleared = false;

        _killCount = 0;
        GameManager.Instance.player.transform.position = Vector2.zero;
        UIManager.Instance.FadeOut(FADE_DURATION / 2);

        _manager.InvokeKillUpdated(_killCount, _requiredCount);
        _manager.InvokeStageChanged(DungeonType.SkillDungeon, StageManager.Instance.skillDungeonLevel, false);
        SkillManager.Instance.RequestResetAllCooldowns();

        int currentLevel = StageManager.Instance.skillDungeonLevel;

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () =>
        {
            SpawnManager.Instance.SpawnEnemiesForDungeon(_requiredCount, DungeonType.SkillDungeon, currentLevel);
        });
    }

    public void OnEnemyDead()
    {
        _killCount++;

        _manager.InvokeKillUpdated(_killCount, _requiredCount);

        if (_killCount >= _requiredCount)
        {
            OnStageClear();
        }
    }

    public void OnStageClear()
    {
        StageManager.Instance.StopTimer();

        //중복 클리어 방지
        if (_isCleared || GameManager.Instance.player.IsDead)
        {
            return;
        }

        _isCleared = true;
        StageManager.Instance.UpdateClearedLevel(DungeonType.SkillDungeon, StageManager.Instance.skillDungeonLevel + 1);

        GameManager.Instance.statSaver.SaveDungeonClearedData(StageManager.Instance.BuildDungeonSaveData()).Forget();

        Sprite[] rewardSprites = new Sprite[_levelData.SpriteKeys.Count];
        int[] amounts = new int[_levelData.Amounts.Count];
        UIRewardSlot[] slots = new UIRewardSlot[_levelData.Amounts.Count];//ObjectPoolManager.Instance.uiPool.GetReward().Init();

        for (int i = 0; i < _levelData.SpriteKeys.Count; i++)
        {
            rewardSprites[i] = DataManager.Instance.GetSpriteByKey(_levelData.SpriteKeys[i]);
            amounts[i] = _levelData.Amounts[i];
            slots[i] = ObjectPoolManager.Instance.uiPool.GetRewardSlot();
            slots[i].Init(_levelData.SpriteKeys[i], _levelData.Amounts[i]);
        }

        //보상 토스트 팝업 띄우기
        ObjectPoolManager.Instance.uiPool.GetReward().Init(rewardSprites, amounts);
        GiveReward(); //플레이어한테 보상 지급
        //티켓 하나 차감
        GameManager.Instance.stats.TrySpendItem(PlayerProgressType.SkillDungeonTicket, 1);
        //다음 단계, 나가기가 있는 팝업 띄우기
        UIStageResultPopup result = UIManager.Instance.PopupOpen<UIStageResultPopup>();

        result.Init(DungeonType.SkillDungeon, true, StageManager.Instance.skillDungeonLevel,
        () =>
        {
            //다음 단계 버튼 눌렀을 때
            if (GameManager.Instance.stats.GetCurrency(PlayerProgressType.SkillDungeonTicket) > 0)
            {
                result.Close();
                StageManager.Instance.skillDungeonLevel++;
                ResetStage();
            }
            else
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_NotEnoughTicket");
            }
        },
        () =>
        {
            //나가기 버튼 눌렀을 때

            result.Close();
            LoadingSceneController.LoadScene("1_StageScene");
        },
        slots);

        GameManager.Instance.statSaver.SetLastActiveNowAsync().Forget();
    }

    public void OnTimeOut()
    {
        OnPlayerDead();
    }

    public void OnPlayerDead()
    {
        StageManager.Instance.StopTimer();
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        UIStageResultPopup result = UIManager.Instance.PopupOpen<UIStageResultPopup>();

        Sprite[] rewardSprites = new Sprite[_levelData.SpriteKeys.Count];
        int[] amounts = new int[_levelData.Amounts.Count];
        UIRewardSlot[] slots = new UIRewardSlot[_levelData.Amounts.Count];//ObjectPoolManager.Instance.uiPool.GetReward().Init();

        for (int i = 0; i < _levelData.SpriteKeys.Count; i++)
        {
            rewardSprites[i] = DataManager.Instance.GetSpriteByKey(_levelData.SpriteKeys[i]);
            amounts[i] = _levelData.Amounts[i];
            slots[i] = ObjectPoolManager.Instance.uiPool.GetRewardSlot();
            slots[i].Init(_levelData.SpriteKeys[i], _levelData.Amounts[i]);
        }

        result.Init(DungeonType.SkillDungeon, false, StageManager.Instance.skillDungeonLevel,
            () =>
            {
                //재시도버튼 눌렀을 때
                result.Close();
                ResetStage();
            },
            () =>
            {
                //나가기 버튼 눌렀을 때
                result.Close();
                LoadingSceneController.LoadScene("1_StageScene");
            },
            slots);
    }

    public void ResetStage()
    {
        UIManager.Instance.FadeIn(FADE_DURATION / 2);
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () =>
        {
            GameManager.Instance.stats.Recovery();
            GameManager.Instance.player.StateMachine.ChangeState(StateType.Idle);
            Start();
        });
    }

    public void GiveReward()
    {
        for (int i = 0; i < _levelData.Currencies.Count; i++)
        {
            PlayerProgressType currency = _levelData.Currencies[i];
            int amount = _levelData.Amounts[i];

            GameManager.Instance.stats.AddCurrency(currency, amount);
            Debug.Log($"지급한 보상 : {currency} {amount}");
        }
    }

    public void GiveUp()
    {
        UIConfirmPopup confirmPopup = UIManager.Instance.PopupOpen<UIConfirmPopup>();
        confirmPopup.Init(() =>
        {
            confirmPopup.Close();
            LoadingSceneController.LoadScene("1_StageScene");
        },
        "UI_CheckGiveUp");
    }
}
