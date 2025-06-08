using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayCallManager : MonoBehaviour
{
    //CallRepeat, CancelCall, IsRunning

    public static DelayCallManager Instance { get; private set; }

    private class DelayTask
    {
        public float time; //남은 시간
        public System.Action callback; //시간 됐을 때 실행할 함수
    }

    //예약된 딜레이 작업들
    private readonly List<DelayTask> taskList = new List<DelayTask>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        //리스트에 추가하면 맨 뒤에 추가되니 뒤에서 앞으로 돌아야 함
        //i는 증가하는데 Count가 줄어들거나 하면 요소가 건너뛰어질 위험도 있음
        for (int i = taskList.Count - 1; i >= 0; i--)
        {
            taskList[i].time -= delta;

            if (taskList[i].time <= 0f)
            {
                taskList[i].callback?.Invoke();
                taskList.RemoveAt(i);
            }
        }
    }

    public void CallLater(float delay, System.Action callback)
    {
        //외부에서 호출하면 taskList에 저장되고, Update에서 Invoke시켜줌
        taskList.Add(new DelayTask
        {
            time = delay,
            callback = callback
        });
    }
}
