using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStageFlow
{
    void Start();

    void OnEnemyDead();

    void OnStageClear();

    void OnTimeOut();

    void OnPlayerDead();

    void ResetStage();

    void GiveReward();

    void GiveUp();
}
