using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    //상태 행동 약속
    //상태마다 OnEnter, Update, OnExit 패턴을 강제해야 switch 지옥 안생김
    void OnEnter();
    void Update();
    void OnExit();
}
