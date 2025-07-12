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

        if (anim != null)
        {
            anim.SetTrigger("4_Death");
        }

        float deadTime = anim.GetCurrentAnimatorStateInfo(0).length;
        DelayCallManager.Instance.CallLater(deadTime, () => { ObjectPoolManager.Instance.enemyPool.Return(owner.enemy); });

        GameManager.Instance.player.GetExp(owner.enemy.expValue);
        GameManager.Instance.player.GetGold(owner.enemy.goldValue);
        GameManager.Instance.enemyList.Remove(owner.enemy);
        StageManager.Instance.NotifyKill();
    }

    public void OnExit()
    {
        owner.enemy.isDead = false;
    }

    public void Update()
    {

    }
}