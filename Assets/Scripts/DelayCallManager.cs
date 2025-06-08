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
        public float time; //���� �ð�
        public System.Action callback; //�ð� ���� �� ������ �Լ�
    }

    //����� ������ �۾���
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

        //����Ʈ�� �߰��ϸ� �� �ڿ� �߰��Ǵ� �ڿ��� ������ ���ƾ� ��
        //i�� �����ϴµ� Count�� �پ��ų� �ϸ� ��Ұ� �ǳʶپ��� ���赵 ����
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
        //�ܺο��� ȣ���ϸ� taskList�� ����ǰ�, Update���� Invoke������
        taskList.Add(new DelayTask
        {
            time = delay,
            callback = callback
        });
    }
}
