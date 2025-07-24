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
        owner.enemy.deadCount++;

        if (owner.enemy.IsBoss)
        {
            Debug.Log($"IsBoss : {owner.enemy.IsBoss}");
        }

        owner.enemy.isDead = true;
        anim = owner.enemy.Animator;

        if (anim != null)
        {
            anim.SetTrigger("4_Death");
        }

        float deadTime = anim.GetCurrentAnimatorStateInfo(0).length;

        DelayCallManager.Instance.CallLater(deadTime, () =>
        {
            owner.enemy.transform.localScale = owner.enemy.OriginScale;
            ObjectPoolManager.Instance.enemyPool.Return(owner.enemy);
        });

        GameManager.Instance.player.GetExp(owner.enemy.expValue);
        GameManager.Instance.player.GetGold(owner.enemy.goldValue);
        GameManager.Instance.enemyList.Remove(owner.enemy);

        if (owner.enemy.IsBoss == false)
        {
            StageManager.Instance.NotifyKill();

        }
        else if (owner.enemy.IsBoss)
        {
            Debug.Log($"BossDead : {owner.enemy.IsBoss}");

            StageManager.Instance.NotifyKillBoss();
        }
    }

    public void OnExit()
    {
        owner.enemy.isDead = false;
    }

    public void Update()
    {

    }
}