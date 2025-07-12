using UnityEngine;

public class EnemyDeadState : IState
{
    private EnemyStateMachine owner;
    private Animator anim;

    public EnemyDeadState(EnemyStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        owner.enemy.isDead = true;
        anim = owner.enemy.Animator;
        float deadTime = anim.GetCurrentAnimatorStateInfo(0).length;
        DelayCallManager.Instance.CallLater(deadTime, () => { ObjectPoolManager.Instance.enemyPool.Return(owner.enemy); });
    }

    public void OnExit()
    {
        owner.enemy.isDead = false;
    }

    public void Update()
    {

    }
}