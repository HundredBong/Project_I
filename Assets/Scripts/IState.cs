using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    //���� �ൿ ���
    //���¸��� OnEnter, Update, OnExit ������ �����ؾ� switch ���� �Ȼ���
    void OnEnter();
    void Update();
    void OnExit();
}
